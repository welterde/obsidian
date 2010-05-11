using System;
using System.Collections.Generic;

namespace obsidian.World.Objects {
	public class StateButton : ActiveRegion {
		private static byte[] blocks = { 21,25,28,23,30 };
		
		private int states;
		private int state = 0;
		
		#region Public members
		public int States {
			get { return states; }
			set {
				if (value<1) { throw new ArgumentOutOfRangeException(); }
				states = value;
			}
		}
		public int State {
			get { return state; }
			set {
				if (value==state) { return; }
				if (value<0 || value>=states) { throw new ArgumentOutOfRangeException(); }
				state = value;
				OnStateChange();
			}
		}
		#endregion
		
		public event Action StateChange = delegate {  };
		
		public StateButton(Level level,int x,int y,int z,int states)
			: this(level,x,y,z,x+1,y+1,z+1,states) {  }
		public StateButton(Level level,int x1,int y1,int z1,int x2,int y2,int z2,int states)
			: base(level,x1,y1,z1,x2,y2,z2) {
			if (states<1) { throw new ArgumentOutOfRangeException("states"); }
			this.states = states;
		}
		
		protected virtual void OnStateChange() {
			Level.Cuboid(null,this,blocks[state%blocks.Length]);
			StateChange();
		}
		protected override void OnBlock(BlockArgs args) {
			args.Abort = true;
			if (Active) {
				state++;
				if (state==states) { state = 0; }
				OnStateChange();
			} base.OnBlock(args);
		}
	}
}
