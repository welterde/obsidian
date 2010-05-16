using System;
using System.Collections.Generic;

namespace obsidian.World.Objects {
	public class Portal {
		private Region first;
		private Region second;
		private byte orientation = 0;
		private List<Player> ignore = new List<Player>();
		
		public Region First {
			get { return first; }
		}
		public Region Second {
			get { return second; }
		}
		public byte Orientation {
			get { return orientation; }
			set { orientation = value; }
		}
		
		public event Action<Body> Used = delegate {  };
		
		public Portal(Level level,BlockArgs first,BlockArgs second)
			: this(level,first.X,first.Y,first.Z,second.X,second.Y,second.Z) {  }
		public Portal(Level level,int x1,int y1,int z1,int x2,int y2,int z2) {
			first = new Region(level,x1,y1,z1,x1+1,y1+2,z1+1);
			second = new Region(level,x2,y2,z2,x2+1,y2+2,z2+1);
			level.Cuboid(null,first,0);
			level.Cuboid(null,second,0);
			first.EnterEvent += OnEnterFirst;
			second.EnterEvent += OnEnterSecond;
			first.BlockEvent += OnBlock;
			second.BlockEvent += OnBlock;
			first.Tag = this;
			second.Tag = this;
		}
		
		public void Destroy() {
			first.Destroy();
			second.Destroy();
		}
		
		private void OnEnterFirst(Body body) {
			OnEnter(body,second);
		}
		private void OnEnterSecond(Body body) {
			OnEnter(body,first);
		}
		private void OnEnter(Body body,Region region) {
			Player player = body as Player;
			if (player!=null) {
				if (!ignore.Contains(player)) {
					ignore.Add(player);
					player.Teleport(
						region.X1*32+region.Width*16,region.Y1*32+region.Depth*16,
						region.Z1*32+region.Height*16,unchecked((byte)(body.Position.RotX+orientation)),body.Position.RotY);
					Used(body);
				} else { ignore.Remove(player); }
			}
		}
		protected void OnBlock(object sender,BlockArgs args) {
			args.Abort = true;
		}
	}
}
