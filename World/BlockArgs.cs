using System;

namespace obsidian.World {
	public class BlockArgs : EventArgs {
		#region Members
		private readonly Player origin;
		private readonly short x,y,z;
		private readonly byte type;
		private bool abort = false;
		#endregion
		
		#region Public members
		public Player Origin {
			get { return origin; }
		}
		public short X {
			get { return x; }
		}
		public short Y {
			get { return y; }
		}
		public short Z {
			get { return z; }
		}
		public byte Type {
			get { return type; }
		}
		public bool Abort {
			get { return abort; }
			set { abort = value; }
		}
		#endregion
		
		public BlockArgs(Player origin,short x,short y,short z,byte type) {
			this.origin = origin;
			this.x = x; this.y = y; this.z = z;
			this.type = type;
		}
	}
}
