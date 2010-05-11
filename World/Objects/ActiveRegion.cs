using System;

namespace obsidian.World.Objects {
	public class ActiveRegion : Region {
		private bool active = true;

		public bool Active {
			get { return active; }
			set {
				if (active==value) { return; }
				active = value;
				if (active) { OnActivated(); }
				else { OnDeactivated(); }
			}
		}
		
		public event Action Activated = delegate {  };
		public event Action Deactivated = delegate {  };
		
		public ActiveRegion(Level level,int x1,int y1,int z1,int x2,int y2,int z2)
			: base(level,x1,y1,z1,x2,y2,z2) {  }
		
		protected virtual void OnActivated() {
			Activated();
		}
		protected virtual void OnDeactivated() {
			Deactivated();
		}
	}
}
