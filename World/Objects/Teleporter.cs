using System;

namespace obsidian.World.Objects {
	public class Teleporter : ActiveRegion {
		private Position target;
		
		public Position Target {
			get { return target; }
			set {
				if (value==null) { throw new ArgumentNullException(); }
				target = value;
			}
		}
		
		public event Action<Body> Used = delegate {  };
		
		public Teleporter(Level level,int x1,int y1,int z1,int x2,int y2,int z2,Position target)
			: base(level,x1,y1,z1,x2,y2,z2) {
			this.target = target;
		}
		
		protected override void OnEnter(Body body) {
			if (body.Player!=null && Active) {
				body.Player.Teleport(target);
				Used(body);
			} base.OnEnter(body);
		}
		protected override void OnBlock(BlockArgs args) {
			args.Abort = true;
			base.OnBlock(args);
		}
	}
}
