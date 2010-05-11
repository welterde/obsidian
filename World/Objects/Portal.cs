using System;
using System.Collections.Generic;

namespace obsidian.World.Objects {
	public class Portal : ActiveRegion {
		private Portal exit = null;
		private byte orientation = 0;
		private List<Body> ignore = new List<Body>();
		
		public Portal Exit {
			get { return exit; }
		}
		public byte Orientation {
			get { return orientation; }
			set { orientation = value; }
		}
		
		public event Action<Body> Used = delegate {  };
		public event Action Connected = delegate {  };
		
		public Portal(Level level,int x1,int y1,int z1,int x2,int y2,int z2)
			: base(level,x1,y1,z1,x2,y2,z2) {  }
		
		public override void Destroy() {
			if (exit!=null) {
				exit.exit = null;
				exit = null;
			} base.Destroy();
		}
		
		protected override void OnEnter(Body body) {
			if (body.Player!=null && Active && exit!=null) {
				if (!ignore.Contains(body)) {
					exit.ignore.Add(body);
					body.Player.Teleport(
						exit.X1*32+exit.Width*16,exit.Y1*32+exit.Depth*16,
						exit.Z1*32+exit.Height*16,unchecked((byte)(body.Position.RotX+orientation)),body.Position.RotY);
					Used(body);
				} else { ignore.Remove(body); }
			} base.OnEnter(body);
		}
		protected override void OnBlock(BlockArgs args) {
			args.Abort = true;
			base.OnBlock(args);
		}
		
		public void Connect(Portal portal) {
			if (portal==null) { throw new ArgumentNullException("portal"); }
			if (portal==this) { throw new ArgumentException("Can't connect to self.","portal"); }
			if (exit==portal) { return; }
			if (exit!=null) { exit.exit = null; exit.Connected(); }
			if (portal.exit!=null) { portal.exit.exit = null; portal.exit.exit.Connected(); }
			portal.exit = this;
			exit = portal;
			portal.Connected();
			Connected();
		}
	}
}
