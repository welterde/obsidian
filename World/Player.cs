using System;
using System.Net;
using System.Threading;
using obsidian.Net;
using obsidian.Control;
using obsidian.Data;

namespace obsidian.World {
	public class Player : Body {
		#region Members
		private readonly Server server;
		private readonly Protocol helper;
		internal Account account;
		private OnlineStatus status = OnlineStatus.Connected;
		private string quitMessage = null;
		private bool destroyAdminium = false;
		internal byte[] bind;
		#endregion
		
		#region Public members
		public Account Account {
			get { return account; }
		}
		public Group Group {
			get { return account.Group; }
			set { account.Group = value; }
		}
		public Node Custom {
			get { return account.Custom; }
		}
		public OnlineStatus Status {
			get { return status; }
		}
		public string IP {
			get { return helper.IPEndPoint.Address.ToString(); }
		}
		public bool DestroyAdminium {
			get { return destroyAdminium; }
			set {
				if (destroyAdminium==value) { return; }
				destroyAdminium = value;
				if (destroyAdminium) { Protocol.OptionPacket(100).Send(this); }
				else { Protocol.OptionPacket(0).Send(this); }
			}
		}
		#endregion
			
		#region Events
		internal event Action<Player> IdentifiedEvent = delegate {  };
		internal event Action<Player,byte,string,string> LoginEvent = delegate {  };
		internal event Action<Player,string> ChatEvent = delegate {  };
		internal event Action<Player,BlockArgs> InternalBlockEvent = delegate {  };
		public event Action<Player> ReadyEvent = delegate {  };
		public event Action<Player,string> DisconnectedEvent = delegate {  };
		public event Action<Player,BlockArgs,byte> BlockEvent = delegate {  };
		#endregion
		
		internal Player(Server server,Protocol helper) {
			this.server = server;
			this.helper = helper;
			helper.Disconnected += OnDisconnect;
			helper.Login += OnLogin;
			helper.Block += OnBlock;
			helper.Move += OnMove;
			helper.Chat += OnChat;
			bind = new byte[Blocktype.Count];
			for (byte i=0;i<bind.Length;i++) { bind[i] = i; }
		}
		
		private void OnDisconnect() {
			if (level!=null) {
				level.players.Remove(this);
				Visible = false;
			} try { DisconnectedEvent(this,quitMessage); }
			catch (Exception e) { server.lua.Error(e); }
			
		}
		private void OnLogin(byte version,string name,string verify,byte mode) {
			if (status!=OnlineStatus.Connected) { return; }
			this.name = name;
			LoginEvent(this,version,name,verify);
			if (helper.Running) {
				status = OnlineStatus.Identified;
				SendLogin(server.Name,server.Motd,0);
				SendLevel(level);
			}
		}
		private void OnBlock(short x,short y,short z,byte action,byte type) {
			if (status!=OnlineStatus.Ready) { return; }
			if (action>=2) { new Message("&eUnknown block action.").Send(this); return; }
			Blocktype blocktype = Blocktype.FindById(type);
			if (blocktype==null) { new Message("&eUnknown blocktype.").Send(this); return; }
			if (!blocktype.Placeable) { new Message("&eUnplaceable blocktype.").Send(this); return; }
			if (y==level.Depth) { return; }
			if (x>=level.Width || y>=level.Depth || z>=level.Height) {
				new Message("&eInvalid block position.").Send(this); return;
			} byte block = type;
			if (action==0) { block = 0x00; }
			byte before = level.GetBlock(x,y,z);
			if (before==block) { return; }
			if (before==Blocktype.adminium.ID && !destroyAdminium) {
				new Message("&eYou can't destroy adminium.").Send(this); return;
			} BlockArgs args = new BlockArgs(this,x,y,z,block);
			try { BlockEvent(this,args,type); }
			catch (Exception e) { server.lua.Error(e); }
			InternalBlockEvent(this,args);
		}
		private void OnMove(byte id,ushort x,ushort y,ushort z,byte rotx,byte roty) {
			if (status!=OnlineStatus.Ready) { return; }
			Position.Set(x,y,z,rotx,roty);
		}
		private void OnChat(byte id,string message) {
			if (status!=OnlineStatus.Ready) { return; }
			ChatEvent(this,message);
		}
		
		internal void Send(byte[] buffer) {
			helper.Send(buffer);
		}
		internal void SendLogin(string name,string motd,byte option) {
			if (status!=OnlineStatus.Identified) { return; }
			Protocol.LoginPacket(name,motd,option).Send(this);
		}
		internal void SendLevel(Level level) {
			if (status!=OnlineStatus.Identified) { return; }
			this.level = level;
			status = OnlineStatus.Loading;
			new Thread(
				delegate() {
					Protocol.MapBeginPacket().Send(this);
					byte[] gzipped = Utility.GZipper.GZip(
						BitConverter.GetBytes(
							IPAddress.HostToNetworkOrder(level.Mapdata.Length)
						),level.Mapdata);
					for (int i=0;i<gzipped.Length;i+=1024) {
						byte progress = (byte)((i/1024+1)/Math.Ceiling(gzipped.Length/1024d)*100);
						Protocol.MapPartPacket(gzipped,i,progress).Send(this);
					} Protocol.MapEndPacket(level.Width,level.Depth,level.Height).Send(this);
					try { ReadyEvent(this); }
					catch (Exception e) { server.lua.Error(e); }
					Spawn(level.Spawn);
					foreach (Body b in level.Bodies) { Protocol.SpawnPacket(b).Send(this); }
					Position.Set(level.Spawn);
					Visible = true;
					status = OnlineStatus.Ready;
					level.players.Add(this);
				}).Start();
		}
		public void Spawn(Position pos) {
			Protocol.SpawnPacket(0xFF,name,pos).Send(this);
		}
		public void Teleport(Position pos) {
			Protocol.TeleportPacket(0xFF,pos).Send(this);
		}
		public void Teleport(int x,int y,int z) {
			Teleport(x,y,z,Position.RotX,Position.RotY);
		}
		public void Teleport(int x,int y,int z,byte rotx,byte roty) {
			unchecked {
				Protocol.TeleportPacket(0xFF,(ushort)x,(ushort)y,(ushort)z,rotx,roty).Send(this);
			}
		}
		public void Kick(string message) {
			quitMessage = message;
			Protocol.KickPacket(message).Send(this);
			helper.Close();
			OnDisconnect();
		}
		
		public enum OnlineStatus {
			Disconnected,
			Connected,
			Identified,
			Loading,
			Ready
		}
	}
}
