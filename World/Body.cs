using System;
using obsidian.Net;

namespace obsidian.World {
	public class Body {
		#region Members
		private readonly string name;
		private readonly Player player = null;
		private readonly Position position = new Position(0,0,0,0,0);
		private readonly Position oldpos = new Position(0,0,0,0,0);
		private readonly Level level;
		private byte id;
		private bool visible = false;
		#endregion
		
		#region Public members
		public string Name {
			get { return name; }
		}
		public Player Player {
			get { return player; }
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
		public bool Visible {
			get { return visible; }
		}
		#endregion
		
		public Body(string name,object bound,Level level) {
			this.name = name;
			if (bound is Player) { player = (Player)bound; }
			this.level = level;
		}
		
		public void Create(Position pos) {
			this.position.Set(pos);
			oldpos.Set(pos);
			Create();
		}
		public void Create() {
			if (visible) { throw new Exception("Body is already visible."); }
			if (level.Bodies.Count>255) { throw new Exception("Too many bodies in one level."); }
			visible = true;
			bool[] used = new bool[255];
			foreach (Body body in Level.Bodies) { used[body.id] = true; }
			for (byte i=0;i<used.Length;i++) { if (!used[i]) { id = i; break; } }
			Protocol.SpawnPacket(this).Send(level);
			level.bodies.Add(this);
		}
		public void Destroy() {
			if (!visible) { throw new Exception("Body isn't visible."); }
			visible = false;
			Protocol.DiePacket(id).Send(level);
			level.bodies.Remove(this);
		}
		
		public void Update() {
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
			}
		}
	}
}
