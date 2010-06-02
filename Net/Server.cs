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
		private string initfile = "init.lua";
		
		private string name = "Custom Minecraft server";
		private string motd = "Welcome to my custom Minecraft server!";
		private ushort port = 25565;
		private bool listed = false;
		private byte slots = 16;
		private string mainLevel = null;
		private string help = "&eTo show a list of commands, type '/help commands'.";
		internal readonly int salt = new Random().Next();
		
		private TextWriter log;
		private Listener listener = new Listener();
		private Heartbeat heartbeat;
		private Thread updateThread;
		internal Lua lua;
		
		private List<Player> connections = new List<Player>();
		private List<Player> players = new List<Player>();
		private Command.List commands;
		private Group.List groups = new Group.List();
		private Account.List accounts = new Account.List();
		private UpdateQueue queue;
		private Level level;
		#endregion
		
		#region Public members
		public string Name {
			get { return name; }
			set {
				if (value==null) { throw new ArgumentNullException(); }
				name = value;
			}
		}
		public string Motd {
			get { return motd; }
			set {
				if (value==null) { throw new ArgumentNullException(); }
				motd = value;
			}
		}
		public ushort Port {
			get { return port; }
			set {
				if (listener.Running) { throw new Exception("Change port while server is running."); }
				port = value;
			}
		}
		public bool Public {
			get { return listed; }
			set { listed = value; }
		}
		public byte Slots {
			get { return slots; }
			set { slots = value; }
		}
		public string MainLevel {
			get { return mainLevel; }
			set { mainLevel = value; }
		}
		public string Help {
			get { return help; }
			set {
				if (value==null) { throw new ArgumentNullException("value"); }
				help = value;
			}
		}
		
		public ReadOnlyCollection<Player> Players {
			get { return players.AsReadOnly(); }
		}
		public Command.List Commands {
			get { return commands; }
		}
		public Group.List Groups {
			get { return groups; }
		}
		public Account.List Accounts {
			get { return accounts; }
		}
		public UpdateQueue Queue {
			get { return queue; }
		}
		public Level Level {
			get { return level; }
		}
		public bool Running {
			get { return listener.Running; }
		}
		#endregion
		
		public event Action<Player,string> PlayerLoginEvent = delegate {  };
		public event Action InitializedEvent = delegate {  };
		
		public Server(TextWriter log) {
			this.log = log;
			listener.AcceptEvent += Accept;
			heartbeat = new Heartbeat(this);
			updateThread = new Thread(UpdateBodies);
			lua = new Lua(this);
			commands = new Command.List(this);
			queue = new UpdateQueue(this);
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
			commands.Init();
			lua.errorLog = "error.log";
			Log("Using initfile '"+initfile+"'.");
			if (!lua.Start(initfile)) { return false; }
			int loaded,failed;
			string error;
			groups.Load(commands,out loaded,out failed,out error);
			Log("{0} group{1} loaded{2}.",loaded<0?-loaded:loaded,loaded==1?"":"s",failed==0?"":" ("+failed+" failed)");
			if (error!=null) { Log("Error: "+error); return false; }
			accounts.Load(groups,out loaded,out failed);
			Log("{0} account{1} loaded{2}.",loaded,loaded==1?"":"s",failed==0?"":" ("+failed+" failed)");
			if (mainLevel==null || !File.Exists("levels/"+mainLevel+".lvl")) {
				log.Write("({0}) Generating Level ... ",DateTime.Now.ToString("HH:mm:ss"));
				level = LevelGenerator.Flatgrass(128,64,128);
				if (level==null) { log.WriteLine("Error!"); return false; }
				log.WriteLine("Done.");
			} else {
				level = World.Level.Load(mainLevel);
				if (level==null) { Log("Failed to load level '"+mainLevel+"'."); return false; }
			} level.server = this;
			if (!listener.Start(port)) {
				Log("Error: Server creation failed, port {0} already in use?",port);
				return false;
			} Log("Server is running on port {0}.",port);
			InitializedEvent.Raise(this);
			heartbeat.Start(1000*55);
			if (heartbeat.Send()) {
				File.WriteAllText("externalurl.txt",heartbeat.url);
				Log("URL saved to externalurl.txt.\n  "+heartbeat.url);
			} updateThread.Start();
			queue.Start();
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
			if (Running) { throw new Exception("Server isn't running"); }
			lua.Stop();
			listener.Stop();
			heartbeat.Stop();
			updateThread.Abort();
			queue.Stop();
			Log("Server stopped.",port);
		}
		
		private void Accept(Socket socket) {
			Player player = new Player(this,new Protocol(socket));
			connections.Add(player);
			player.LoginEvent += PlayerLogin;
			player.InternalBlockEvent += PlayerBlock;
			player.ChatEvent += PlayerChat;
			player.CommandEvent += PlayerCommand;
			player.DisconnectedEvent += PlayerDisconnected;
			Log("{0} connected.",player.IP);
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
			else if (players.Count>=slots) { player.Kick("Server is full"); }
			else if (player.IP!="127.0.0.1" &&
			         (verify == "--" || !verify.Equals(
			         	BitConverter.ToString(
			         		md5.ComputeHash(Encoding.ASCII.GetBytes(salt+name))).
			         	Replace("-","").TrimStart('0'),StringComparison.OrdinalIgnoreCase))) {
				player.Kick("Login failed! Try again");
			} else if (!(accounts[name]==null ? groups.Standard : accounts[name].Group).CanJoin) {
				player.Kick("You're not allowed to join");
			} else {
				players.ForEach(delegate(Player found) {
				                	if (found.Name.Equals(name,StringComparison.OrdinalIgnoreCase))
				                		found.Kick("Someone logged in as you");
				                });
				Log("{0} logged in as {1}.",player.IP,name);
				connections.Remove(player);
				players.Add(player);
				player.account = accounts.Login(player,name);
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
		private void PlayerChat(Player player,string message) {
			if (message=="") { return; }
			Log("<{0}> {1}",player.Name,message);
			new Message(player.Group.Prefix+player.Name+player.Group.Postfix+": &f"+message).Send(level);
		}
		private void PlayerCommand(Player player,string message,bool byPlayer) {
			if (byPlayer) { Log(player.Name+" used /"+message+"."); }
			string cmd = message.Split(new char[]{' '},2)[0];
			Command command = commands.Search(ref message);
			if (command==null) { throw new CommandException("The command '"+cmd+"' doesn't exist."); }
			else if (player.Group.Commands.Contains(command) || !byPlayer) {
				command.use.Raise(this,command,player,message);
			} else { throw new CommandException("You're not allowed to use '"+command.Name+"'."); }
		}
		private void PlayerDisconnected(Player player,string message) {
			if (player.Status>=Player.OnlineStatus.Identified) {
				Log("{0} disconnected{1}.",player.Name,message==null?"":" ("+message+")");
				players.Remove(player);
				accounts.Logout(player.account);
			} else {
				Log("{0} disconnected{1}.",player.IP,message==null?"":" ("+message+")");
				connections.Remove(player);
			}
		}
		
		public void Log(string value) {
			log.WriteLine(DateTime.Now.ToString("(HH:mm:ss) ")+value);
		}
		public void Log(string format,params object[] arg) {
			log.WriteLine(DateTime.Now.ToString("(HH:mm:ss) ")+format,arg);
		}
	}
	class CommandException : Exception {
		public CommandException(string message) : base(message) {  }
	}
}
