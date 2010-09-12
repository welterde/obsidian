using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using obsidian.World;
using obsidian.Utility;
using obsidian.Control;

namespace obsidian.Net {
	public class Server {
		private static readonly MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
		
		#region Members
		private string initfile = "scripts/init.lua";
		private ushort port = 25565;
		internal readonly int salt = new Random().Next();
		
		private TextWriter log;
		private bool logNewline = true;
		private Listener listener = new Listener();
		private Heartbeat heartbeat;
		private Thread updateThread;
		internal Lua lua;
		
		private List<Player> connections = new List<Player>();
		private List<Player> players = new List<Player>();
		private Level level;
		#endregion
		
		#region Public members
		public string Name { get; set; }
		public string Motd { get; set; }
		public ushort Port {
			get { return port; }
			set {
				if (Running) { throw new Exception("Can't change port while server is running."); }
				port = value;
			}
		}
		public bool Public { get; set; }
		public bool Verify { get; set; }
		public byte Slots { get; set; }
		public string Help { get; set; }
		
		public ReadOnlyCollection<Player> Players {
			get { return players.AsReadOnly(); }
		}
		public Command.List Commands { get; private set; }
		public Group.List Groups { get; private set; }
		public Account.List Accounts { get; private set; }
		public UpdateQueue Queue { get; private set; }
		public Level Level {
			get { return level; }
			set {
				if (Running) { throw new InvalidOperationException("Can't change level while server is running."); }
				level = value;
			}
		}
		public bool Running {
			get { return listener.Running; }
		}
		#endregion
		
		public event Action<Player,string> PlayerLoginEvent = delegate {  };
		public event Action InitializedEvent = delegate {  };
		
		public Server(TextWriter log) {
			this.log = log;
			
			Name = "Custom Minecraft server";
			Motd = "Welcome to my custom Minecraft server!";
			Public = false;
			Verify = true;
			Slots = 16;
			Help = "&eTo show a list of commands, type '/help commands'.";
			
			listener.AcceptEvent += Accept;
			heartbeat = new Heartbeat(this);
			updateThread = new Thread(UpdateBodies);
			lua = new Lua(this);
			Commands = new Command.List(this);
			Groups = new Group.List();
			Accounts = new Account.List();
			Queue = new UpdateQueue(this);
		}
		
		public bool Start() {
			return Start(new string[0]);
		}
		public bool Start(string[] args) {
			if (Running) { throw new Exception("Server is already running"); }
			log.WriteLine();
			log.WriteLine("   = Minecraft Server \"obsidian\" =");
			log.WriteLine();
			if (!ParseParameters(args)) { return false; }
			Commands.Init();
			lua.errorLog = "error.log";
			Log("Using initfile '"+initfile+"'.");
			if (!lua.Start(initfile)) { return false; }
			string error;
			Log("",false); Groups.Load(Commands,out error); Log("");
			if (error!=null) { Log("Error: "+error); return false; }
			Log("",false); Accounts.Load(Groups); Log("");
			if (level==null) {
				Log("Error: No level loaded.");
				return false;
			} level.server = this;
			if (!listener.Start(port)) {
				Log("Error: Server creation failed, port "+port+" already in use?");
				return false;
			} Log("Server is running on port "+port+".");
			InitializedEvent.Raise(this);
			heartbeat.Start(1000*55);
			if (heartbeat.Send()) {
				File.WriteAllText("externalurl.txt",heartbeat.url);
				Log("URL saved to externalurl.txt.\n  "+heartbeat.url);
			} updateThread.Start();
			Queue.Start();
			return true;
		}
		private bool ParseParameters(string[] args) {
			for (int i=0;i<args.Length;i++) {
				switch (args[i]) {
					case "-init":
						i++; if (i>=args.Length) {
							Log("Could not parse parameter '-init'.");
							return false;
						} initfile = args[i];
						break;
					default:
						Log("Unknown parameter '"+args[i]+"'.");
						return false;
				}
			} return true;
		}
		public void Stop() {
			if (!Running) { throw new Exception("Server isn't running"); }
			lua.Stop();
			listener.Stop();
			heartbeat.Stop();
			updateThread.Abort();
			Queue.Stop();
			Log("Server stopped.");
		}
		
		private void Accept(Socket socket) {
			Player player = new Player(this,new Protocol(socket));
			connections.Add(player);
			player.LoginEvent += PlayerLogin;
			player.InternalBlockEvent += PlayerBlock;
			player.CommandEvent += PlayerCommand;
			player.DisconnectedEvent += PlayerDisconnected;
			Log(player.IP+" connected.");
		}
		private void UpdateBodies() {
			int counter = 0;
			while (true) {
				Thread.Sleep(30);
				Action action = delegate {
					foreach (Body body in new List<Body>(level.Bodies)) { body.Update(); }
					if (++counter>=6) foreach (Region region in new List<Region>(level.Regions)) { region.Update(); }
				}; action.Raise(this);
			}
		}
		
		private void PlayerLogin(Player player,byte version,string name,string verify) {
			if (version!=Protocol.version) { player.Kick("Wrong version"); }
			else if (!RegexHelper.IsValidName(name)) { player.Kick("Invalid name"); }
			else if (!(Accounts[name]==null ? Groups.Standard : Accounts[name].Group).CanJoinFull && players.Count>=Slots) {
				player.Kick("Server is full");
			} else if (!Verify && player.IP!="127.0.0.1" &&
			           (verify == "--" || !verify.Equals(
			           	BitConverter.ToString(
			           		md5.ComputeHash(Encoding.ASCII.GetBytes(salt+name))).
			           	Replace("-","").TrimStart('0'),StringComparison.OrdinalIgnoreCase))) {
				player.Kick("Login failed! Try again");
			} else if (!(Accounts[name]==null ? Groups.Standard : Accounts[name].Group).CanJoin) {
				player.Kick("You're not allowed to join");
			} else {
				players.ForEach(delegate(Player found) {
				                	if (found.Name.Equals(name,StringComparison.OrdinalIgnoreCase))
				                		found.Kick("Someone logged in as you");
				                });
				Log(player.IP+" logged in as "+name+".");
				connections.Remove(player);
				players.Add(player);
				player.account = Accounts.Login(player,name);
				player.level = level;
				PlayerLoginEvent.Raise(this,player,name);
			}
		}
		private void PlayerBlock(Player player,BlockArgs e,bool sendPlayer) {
			if (sendPlayer) {
				if (!player.level.SetBlock(player,e.X,e.Y,e.Z,e.Type))
					Protocol.BlockPacket(e.X,e.Y,e.Z,player.level[e.X,e.Y,e.Z]).Send(player);
			} else { player.level.PlayerSetBlock(player,e.X,e.Y,e.Z,e.Type); }
		}
		private void PlayerCommand(Player player,string message,bool byPlayer) {
			if (byPlayer) { Log(player.Name+" used /"+message+"."); }
			string cmd = message.Split(new char[]{' '},2)[0];
			Command command = Commands.Search(ref message);
			if (command==null) { throw new CommandException("The command '"+cmd+"' doesn't exist."); }
			else if (player.Group.Commands.Contains(command) || !byPlayer) {
				command.use.Raise(this,command,player,message);
			} else { throw new CommandException("You're not allowed to use '"+command.Name+"'."); }
		}
		private void PlayerDisconnected(Player player,string message) {
			if (player.Status>=Player.OnlineStatus.Identified) {
				Log(player.Name+" disconnected"+(message==null?"":" ("+message+")")+".");
				players.Remove(player);
				Accounts.Logout(player.account);
			} else {
				Log(player.IP+" disconnected"+(message==null?"":" ("+message+")")+".");
				connections.Remove(player);
			}
		}
		
		public void Log(string value) {
			Log(value,true);
		}
		public void Log(string value,bool newline) {
			lock (log) {
				if (logNewline) { value = DateTime.Now.ToString("(HH:mm:ss) ")+value; }
				log.Write(value);
				if (newline) { log.WriteLine(); }
				logNewline = newline;
			}
		}
	}
	class CommandException : Exception {
		public CommandException(string message) : base(message) {  }
	}
}
