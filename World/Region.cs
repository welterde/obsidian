using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using obsidian.Utility;

namespace obsidian.World {
	public class Region {
		#region Members
		private readonly Level level;
		private readonly List<Body> inside = new List<Body>();
		private int x1,y1,z1,x2,y2,z2;
		private object tag;
		#endregion
		
		#region Public Members
		public Level Level {
			get { return level; }
		}
		public ReadOnlyCollection<Body> Inside {
			get { return inside.AsReadOnly(); }
		}
		public int X1 {
			get { return x1; }
			set { x1 = value; }
		}
		public int Y1 {
			get { return y1; }
			set { y1 = value; }
		}
		public int Z1 {
			get { return z1; }
			set { z1 = value; }
		}
		public int X2 {
			get { return x2; }
			set { x2 = value; }
		}
		public int Y2 {
			get { return y2; }
			set { y2 = value; }
		}
		public int Z2 {
			get { return z2; }
			set { z2 = value; }
		}
		public int Width {
			get { return x2-x1; }
			set { x2 = x1 + value; }
		}
		public int Depth {
			get { return y2-y1; }
			set { y2 = y1 + value; }
		}
		public int Height {
			get { return z2-z1; }
			set { z2 = z1 + value; }
		}
		public Position Center {
			get { return new Position(x1*32+Width*16,y1*32+Depth*16,z1*32+Height*16,0,0); }
		}
		public object Tag {
			get { return tag; }
			set { tag = value; }
		}
		#endregion
		
		#region Events
		public event EventHandler<BlockArgs> BlockEvent = delegate {  };
		public event Action<Body> EnterEvent = delegate {  };
		public event Action<Body> LeaveEvent = delegate {  };
		public event Action Destroyed = delegate {  };
		#endregion
		
		public Region(Level level,int x1,int y1,int z1,int x2,int y2,int z2) {
			if (level==null) { throw new ArgumentNullException("level"); }
			this.level = level;
			this.x1 = x1; this.y1 = y1; this.z1 = z1;
			this.x2 = x2; this.y2 = y2; this.z2 = z2;
			level.regions.Add(this);
			level.BlockEvent += CheckBlock;
		}
		
		public virtual void Destroy() {
			level.regions.Remove(this);
			level.BlockEvent -= CheckBlock;
			Destroyed.Raise(level.server);
		}
		
		protected virtual void OnEnter(Body body) {
			EnterEvent.Raise(level.server,body);
		}
		protected virtual void OnLeave(Body body) {
			LeaveEvent.Raise(level.server,body);
		}
		protected virtual void OnBlock(BlockArgs args) {
			BlockEvent.Raise(level.server,this,args);
		}
		
		private void CheckBlock(object sender,BlockArgs args) {
			if (IsInside(args.X,args.Y,args.Z)) { OnBlock(args); }
		}
		
		internal void Update() {
			foreach (Body body in new List<Body>(level.Bodies)) {
				bool isInside = IsInside(body);
				bool contains = inside.Contains(body);
				if (isInside && !contains) {
					inside.Add(body);
					OnEnter(body);
				} else if (!isInside && contains) {
					inside.Remove(body);
					OnLeave(body);
				}
			}
		}
		
		public bool Overlaps(Region region) {
			return (region.x1>x1 || region.y1>y1 || region.x1>y1 ||
			        region.x2<x2 || region.y2<y2 || region.z2<z2);
		}

		public bool IsInside(Body body) {
			return (body.Position.X>=x1*32 && body.Position.Y>=y1*32 && body.Position.Z>=z1*32 &&
			        body.Position.X<x2*32 && body.Position.Y<y2*32+48 && body.Position.Z<z2*32);
		}
		public bool IsInside(Position pos) {
			return (pos.X>=x1*32 && pos.Y>=y1*32 && pos.Z>=z1*32 &&
			        pos.X<x2*32 && pos.Y<y2*32 && pos.Z<z2*32);
		}
		public bool IsInside(Region region) {
			return (region.x1>=x1 && region.y1>=y1 && region.z1>=z1 &&
			        region.x2<=x2 && region.y2<=y2 && region.z2<=z2);
		}
		public bool IsInside(int x,int y,int z) {
			return (x>=x1 && y>=y1 && z>=z1 && x<x2 && y<y2 && z<z2);
		}
	}
}
