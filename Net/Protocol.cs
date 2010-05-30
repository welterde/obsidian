using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using obsidian.World;

namespace obsidian.Net {
	// Can't touch this. Duu du du dum.. what?
	public class Protocol {
		internal const byte version = 7;
		
		private object myLock = new object();
		private bool running = true;
		private Socket socket;
		private byte[] message;
		private IPEndPoint ipEndPoint;
		
		internal bool Running {
			get { return running; }
		}
		internal IPEndPoint IPEndPoint {
			get { return ipEndPoint; }
		}
		
		#region Events
		internal delegate void DisconnectHandler();
		internal delegate void LoginHandler(byte version,string name,string verify,byte mode);
		internal delegate void BlockHandler(short x,short y,short z,byte action,byte type);
		internal delegate void MoveHandler(byte id,ushort x,ushort y,ushort z,byte rotx,byte roty);
		internal delegate void ChatHandler(byte id,string message);
		internal event DisconnectHandler Disconnected = delegate {  };
		internal event LoginHandler Login = delegate {  };
		internal event BlockHandler Block = delegate {  };
		internal event MoveHandler Move = delegate {  };
		internal event ChatHandler Chat = delegate {  };
		#endregion
		
		internal Protocol(Socket socket) {
			this.socket = socket;
			ipEndPoint = (IPEndPoint)socket.RemoteEndPoint;
			BeginRead(1,HandleID);
		}
		internal void Close() {
			if (!running) { return; }
			running = false;
			socket.Close(200);
		}
		
		private void BeginRead(int length,Action<IAsyncResult> method) {
			if (!running) { return; }
			message = new byte[length];
			try { socket.BeginReceive(message,0,length,SocketFlags.None,new AsyncCallback(method),null); }
			catch (SocketException) { Disconnect(); }
		}
		private void EndRead(IAsyncResult result) {
			if (!running) { return; }
			try { if (socket.EndReceive(result)==0) { Disconnect(); } }
			catch (SocketException) { Disconnect(); }
		}
		
		internal void Send(byte[] buffer) {
			if (!running) { return; }
			try { socket.BeginSend(buffer,0,buffer.Length,SocketFlags.None,null,null); }
			catch (SocketException) { Disconnect(); }
		}
		
		#region Packing
		public static Packet LoginPacket(string name,string motd,byte option) {
			byte[] array = new byte[131];
			array[0] = 0x00;
			array[1] = version;
			CopyTo(name,array,2);
			CopyTo(motd,array,66);
			array[130] = option;
			return new Packet(array);
		}
		public static Packet MapBeginPacket() {
			return new Packet(new byte[1]{0x02});
		}
		public static Packet MapPartPacket(byte[] gzipped,int offset,byte progress) {
			byte[] array = new byte[1028];
			array[0] = 0x03;
			ushort length = (ushort)Math.Min(1024,gzipped.Length-offset);
			CopyTo(length,array,1);
			Buffer.BlockCopy(gzipped,offset,array,3,length);
			array[1027] = progress;
			return new Packet(array);
		}
		public static Packet MapEndPacket(short width,short depth,short height) {
			byte[] array = new byte[7];
			array[0] = 0x04;
			CopyTo(width,array,1);
			CopyTo(depth,array,3);
			CopyTo(height,array,5);
			return new Packet(array);
		}
		public static Packet BlockPacket(short x,short y,short z,byte type) {
			byte[] array = new byte[8];
			array[0] = 0x06;
			CopyTo(x,array,1);
			CopyTo(y,array,3);
			CopyTo(z,array,5);
			array[7] = type;
			return new Packet(array);
		}
		public static Packet SpawnPacket(Body body) {
			return SpawnPacket(body.ID,body.Name,body.Position);
		}
		public static Packet SpawnPacket(byte id,string name,World.Position pos) {
			return SpawnPacket(id,name,(ushort)pos.X,(ushort)pos.Y,(ushort)pos.Z,pos.RotX,pos.RotY);
		}
		public static Packet SpawnPacket(byte id,string name,ushort x,ushort y,ushort z,byte rotx,byte roty) {
			byte[] array = new byte[74];
			array[0] = 0x07;
			array[1] = id;
			CopyTo(name,array,2);
			CopyTo(x,array,66);
			CopyTo(y,array,68);
			CopyTo(z,array,70);
			array[72] = rotx;
			array[73] = roty;
			return new Packet(array);
		}
		public static Packet TeleportPacket(byte id,World.Position pos) {
			return TeleportPacket(id,(ushort)pos.X,(ushort)pos.Y,(ushort)pos.Z,pos.RotX,pos.RotY);
		}
		public static Packet TeleportPacket(byte id,ushort x,ushort y,ushort z,byte rotx,byte roty) {
			byte[] array = new byte[10];
			array[0] = 0x08;
			array[1] = id;
			CopyTo(x,array,2);
			CopyTo(y,array,4);
			CopyTo(z,array,6);
			array[8] = rotx;
			array[9] = roty;
			return new Packet(array);
		}
		public static Packet MoveLookPacket(byte id,byte x,byte y,byte z,byte rotx,byte roty) {
			return new Packet(new byte[7]{0x09,id,x,y,z,rotx,roty});
		}
		public static Packet MovePacket(byte id,byte x,byte y,byte z) {
			return new Packet(new byte[5]{0x0A,id,x,y,z});
		}
		public static Packet LookPacket(byte id,byte rotx,byte roty) {
			return new Packet(new byte[4]{0x0B,id,rotx,roty});
		}
		public static Packet DiePacket(byte id) {
			return new Packet(new byte[2]{0x0C,id});
		}
		public static Packet ChatPacket(string message) {
			return ChatPacket(0x00,message);
		}
		public static Packet ChatPacket(byte id,string message) {
			byte[] array = new byte[66];
			array[0] = 0x0D;
			array[1] = id;
			CopyTo(message,array,2);
			return new Packet(array);
		}
		public static Packet KickPacket(string message) {
			byte[] array = new byte[131];
			array[0] = 0x0E;
			CopyTo(message,array,1);
			return new Packet(array);
		}
		public static Packet OptionPacket(byte option) {
			return new Packet(new byte[2]{0x0F,option});
		}
		#endregion
		
		#region Handling
		private void Disconnect() {
			lock (myLock) {
				if (!running) { return; }
				Close();
			} Disconnected();
		}
		protected void HandleID(IAsyncResult result) {
			EndRead(result);
			if (!running) { return; }
			switch (message[0]) {
					case 0x00: BeginRead(130,HandleLogin); break;
					case 0x01: BeginRead(1,HandleID); break;
					case 0x05: BeginRead(8,HandleBlock); break;
					case 0x08: BeginRead(9,HandleMove); break;
					case 0x0D: BeginRead(65,HandleChat); break;
					default: Disconnect(); return;
			}
		}
		protected void HandleLogin(IAsyncResult result) {
			EndRead(result);
			if (!running) { return; }
			Login(message[0],GetString(message,1),GetString(message,65),message[129]);
			BeginRead(1,HandleID);
		}
		protected void HandleBlock(IAsyncResult result) {
			EndRead(result);
			if (!running) { return; }
			Block(GetInt16(message,0),GetInt16(message,2),GetInt16(message,4),
			      message[6],message[7]);
			BeginRead(1,HandleID);
		}
		protected void HandleMove(IAsyncResult result) {
			EndRead(result);
			if (!running) { return; }
			Move(message[0],GetUInt16(message,1),GetUInt16(message,3),GetUInt16(message,5),
			     message[7],message[8]);
			BeginRead(1,HandleID);
		}
		protected void HandleChat(IAsyncResult result) {
			EndRead(result);
			if (!running) { return; }
			Chat(message[0],GetString(message,1));
			BeginRead(1,HandleID);
		}
		#endregion
		
		private static void CopyTo(short value,byte[] array,int index){
			array[index] = (byte)(value/256);
			array[index+1] = (byte)(value%256);
		}
		private static void CopyTo(ushort value,byte[] array,int index){
			array[index] = (byte)(value/256);
			array[index+1] = (byte)(value%256);
		}
		private static void CopyTo(string value,byte[] array,int index){
			Encoding.ASCII.GetBytes(value.PadRight(64).Substring(0,64)).CopyTo(array,index);
		}
		private static short GetInt16(byte[] array,int index) {
			return (short)(array[index+1]+array[index]*256);
		}
		private static ushort GetUInt16(byte[] array,int index) {
			return (ushort)(array[index+1]+array[index]*256);
		}
		private static string GetString(byte[] array,int index) {
			return Encoding.ASCII.GetString(array,index,64).TrimEnd();
		}
	}
}
