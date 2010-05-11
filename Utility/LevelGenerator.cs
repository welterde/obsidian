using System;
using obsidian.World;

namespace obsidian.Utility {
	public static class LevelGenerator {
		public static Level Flatgrass(short width,short depth,short height) {
			return Flatgrass(width,depth,height,0);
		}
		public static Level Flatgrass(short width,short depth,short height,int grasslevel) {
			if (!ValidLevelSize(width,depth,height)) { return null; }
			Level level = new Level(width,depth,height);
			grasslevel += depth/2-1;
			if (grasslevel>=depth) { return level; }
			int x,y=grasslevel,z;
			for (x=0;x<width;x++)
				for (z=0;z<height;z++)
					level[x,y,z] = 0x02;
			for (y=0;y<grasslevel && y<depth;y++)
				for (x=0;x<width;x++)
					for (z=0;z<height;z++)
						level[x,y,z] = 0x03;
			for (y=0;y<grasslevel-4 && y<depth;y++)
				for (x=0;x<width;x++)
					for (z=0;z<height;z++)
						level[x,y,z] = 0x01;
			return level;
		}
		
		public static bool ValidLevelSize(short width,short depth,short height) {
			return (width>0 && depth>0 && height>0 &&
			        width%16==0 && depth%16==0 && height%16==0 &&
			        width*depth*height<=134217728);
		}
	}
}
