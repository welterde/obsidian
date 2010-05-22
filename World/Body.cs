using System;
using obsidian.Net;
using obsidian.Utility;

namespace obsidian.World {
	public class Body {
		#region Members
		protected string name;
		private readonly Position position = new Position(0,0,0,0,0);
		private readonly Position oldpos = new Position(0,0,0,0,0);
		internal Level level;
		private byte id;
		private bool visible = false;
		#endregion
		
		#region Public members
		public string Name {
			get { return name; }
		}
		public Position Position {
			get { return position; }
		}
		public Level Level {
			get { return level; }
		}
		public byte ID {
			get {
				if (!visible) { throw new Exception("Body isn't visible."); }
				return id;
			}
		}
		public virtual bool Visible {
			get { return visible; }
			set {
				if (value==visible) { return; }
				if (value) { Create(true); }
				else { Destroy(true); }
			}
		}
		#endregion
		
		#region Events
		public event Action<Body> Created = delegate {  };
		public event Action<Body> Destroyed = delegate {  };
		public event Action<Body> MoveEvent = delegate {  };
		#endregion
		
		internal Body() {  }
		public Body(string name,Level level) {
			if (name==null) { throw new ArgumentNullException("name"); }
			if (level==null) { throw new ArgumentNullException("name"); }
			this.name = name;
			this.level = level;
		}
		
		private void Create(bool ev) {
			if (name==null) { throw new Exception("Name is null."); }
			if (level==null) { throw new Exception("Level is null."); }
			if (level.Bodies.Count>255) { throw new Exception("Too many bodies in one level."); }
			visible = true;
			bool[] used = new bool[255];
			foreach (Body body in Level.Bodies) { used[body.id] = true; }
			for (byte i=0;i<used.Length;i++) { if (!used[i]) { id = i; break; } }
			Protocol.SpawnPacket(this).Send(level);
			level.bodies.Add(this);
			if (ev) { Created.Raise(level.server,this); }
		}
		private void Destroy(bool ev) {
			visible = false;
			Protocol.DiePacket(id).Send(level);
			level.bodies.Remove(this);
			if (ev) { Destroyed.Raise(level.server,this); }
		}
		
		internal void Update() {
			if (!visible) { return; }
			Packet packet = null;
			bool posChanged = false,farChanged = false,rotChanged = false;
			if (position.X!=oldpos.X || position.Y!=oldpos.Y || position.Z!=oldpos.Z) { posChanged = true; }
			if (position.RotX!=oldpos.RotX || position.RotY!=oldpos.RotY) { rotChanged = true; }
			if (Math.Abs((int)position.X-oldpos.X)>127 || Math.Abs((int)position.Y-oldpos.Y)>127 ||
			    Math.Abs((int)position.Z-oldpos.Z)>127) { farChanged = true; }
			if (farChanged) {
				packet = Protocol.TeleportPacket(id,Position);
			} else if (posChanged&&rotChanged) unchecked {
				packet = Protocol.MoveLookPacket(
					id,(byte)(position.X-oldpos.X),(byte)(position.Y-oldpos.Y),
					(byte)(position.Z-oldpos.Z),position.RotX,position.RotY);
			} else if (posChanged) unchecked {
				packet = Protocol.MovePacket(
					id,(byte)(position.X-oldpos.X),(byte)(position.Y-oldpos.Y),(byte)(position.Z-oldpos.Z));
			} else if (rotChanged) {
				packet = Protocol.LookPacket(id,position.RotX,position.RotY);
			} if (packet!=null) {
				oldpos.Set(position);
				packet.Send(level);
				MoveEvent.Raise(level.server,this);
			}
		}
	}
}
