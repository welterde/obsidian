using System;

namespace obsidian.World {
	public class Position : ICloneable {
		private int x,y,z;
		private byte rotx,roty;
		
		public int X {
			get { return x; }
			set { x = value; }
		}
		public int Y {
			get { return y; }
			set { y = value; }
		}
		public int Z {
			get { return z; }
			set { z = value; }
		}
		public byte RotX {
			get { return rotx; }
			set { rotx = value; }
		}
		public byte RotY {
			get { return roty; }
			set { roty = value; }
		}
		
		public Position(int x,int y,int z,byte rotx,byte roty) {
			this.x = x; this.y = y; this.z = z;
			this.rotx = rotx; this.roty = roty;
		}
		
		public void Set(Position pos) {
			Set(pos.x,pos.y,pos.z,pos.rotx,pos.roty);
		}
		public void Set(int x,int y,int z,byte rotx,byte roty) {
			Set(x,y,z); Set(rotx,roty);
		}
		public void Set(int x,int y,int z) {
			this.x = x; this.y = y; this.z = z;
		}
		public void Set(byte rotx,byte roty) {
			this.rotx = rotx; this.roty = roty;
		}
		public void SetRelative(int x,int y,int z) {
			this.x += x; this.y += y; this.z += z;
		}
		public double DistanceTo(Position pos) {
			double dx = Math.Pow(x-pos.x,2);
			double dy = Math.Pow(y-pos.y,2);
			double dz = Math.Pow(z-pos.z,2);
			return Math.Sqrt(dx+dy+dz);
		}
		
		public object Clone() {
			return new Position(x,y,z,rotx,roty);
		}
	}
}
