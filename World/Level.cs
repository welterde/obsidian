using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using obsidian.Control;
using obsidian.Net;
using obsidian.Data;
using obsidian.Utility;

namespace obsidian.World {
	public class Level {
		private static DataHost host = NbtHost.Instance;
		
		#region Members
		internal Server server;
		private short width;
		private short depth;
		private short height;
		private byte[] mapdata;
		private byte[] blockdata;
		internal List<Body> bodies = new List<Body>();
		internal List<Player> players = new List<Player>();
		internal List<Region> regions = new List<Region>();
		private Position spawn;
		private Node custom = new Node();
		
		internal byte this[int x,int y,int z] {
			get { return mapdata[x+z*width+y*width*height]; }
			set { mapdata[x+z*width+y*width*height] = value; }
		}
		#endregion
		
		#region Public members
		public short Width {
			get { return width; }
		}
		public short Depth {
			get { return depth; }
		}
		public short Height {
			get { return height; }
		}
		public byte[] Mapdata {
			get { return mapdata; }
		}
		public byte[] Blockdata {
			get { return blockdata; }
		}
		public ReadOnlyCollection<Body> Bodies {
			get { return bodies.AsReadOnly(); }
		}
		public ReadOnlyCollection<Player> Players {
			get { return players.AsReadOnly(); }
		}
		public ReadOnlyCollection<Region> Regions {
			get { return regions.AsReadOnly(); }
		}
		public Position Spawn {
			get { return spawn; }
		}
		public Node Custom {
			get { return custom; }
		}
		#endregion
		
		public event EventHandler<BlockArgs> BlockEvent = delegate {  };
		public event Action<Level> Saving = delegate {  };
		
		public Level(short width,short depth,short height) {
			this.width = width;
			this.depth = depth;
			this.height = height;
			mapdata = new byte[width*depth*height];
			blockdata = new byte[width*depth*height];
			spawn = new Position(width*16+16,depth*16+32,height*16+16,0,0);
		}
		
		#region Manipulation
		public byte GetBlock(short x,short y,short z) {
			return this[x,y,z];
		}
		public bool SetBlock(Player player,short x,short y,short z,byte type) {
			if (this[x,y,z]==type) { return false; }
			BlockArgs e = new BlockArgs(player,x,y,z,type);
			BlockEvent.Raise(server,this,e);
			if (e.Abort) { return false; }
			this[x,y,z] = type;
			Protocol.BlockPacket(x,y,z,type).Send(this);
			return true;
		}
		internal void PlayerSetBlock(Player player,short x,short y,short z,byte type) {
			byte before = this[x,y,z];
			BlockArgs e = new BlockArgs(player,x,y,z,type);
			BlockEvent.Raise(server,this,e);
			if (before==this[x,y,z]) {
				if (e.Abort) { Protocol.BlockPacket(x,y,z,before).Send(player); }
				else { this[x,y,z] = type; Protocol.BlockPacket(x,y,z,type).Send(this,player); }
			}
		}

		public byte GetBlockData(short x,short y,short z) {
			return blockdata[x+z*width+y*width*height];
		}
		public void SetBlockData(short x,short y,short z,byte data) {
			blockdata[x+z*width+y*width*height] = data;
		}
		#endregion
		
		#region Advanced manipulation
		public List<Region> RegionsAt(int x,int y,int z) {
			List<Region> regs = new List<Region>();
			foreach (Region region in regions) {
				if (region.IsInside(x,y,z)) { regs.Add(region); }
			} return regs;
		}
		public List<Region> RegionsAt(Position pos) {
			List<Region> regs = new List<Region>();
			foreach (Region region in regions) {
				if (region.IsInside(pos)) { regs.Add(region); }
			} return regs;
		}
		public List<Region> RegionsAt(Region region) {
			List<Region> regs = new List<Region>();
			foreach (Region reg in regions) {
				if (reg.Overlaps(region)) { regs.Add(reg); }
			} return regs;
		}
		public void Cuboid(Player player,Region region,byte type) {
			Cuboid(player,region,type,DrawMode.Solid);
		}
		public void Cuboid(Player player,Region region,byte type,DrawMode mode) {
			Cuboid(player,region.X1,region.Y1,region.Z1,
			       region.X2,region.Y2,region.Z2,type);
		}
		public void Cuboid(Player player,int x1,int y1,int z1,int x2,int y2,int z2,byte type) {
			Cuboid(player,x1,y1,z1,x2,y2,z2,type,DrawMode.Solid);
		}
		public void Cuboid(Player player,int x1,int y1,int z1,int x2,int y2,int z2,byte type,DrawMode mode) {
			x1 = Math.Max(x1,0); y1 = Math.Max(y1,0); z1 = Math.Max(z1,0);
			x2 = Math.Min(x2,width); y2 = Math.Min(y2,depth); z2 = Math.Min(z2,height);
			switch (mode) {
				case DrawMode.Solid:
					for (int x=x1;x<x2;x++)
						for (int y=y1;y<y2;y++)
							for (int z=z1;z<z2;z++)
								SetBlock(player,(short)x,(short)y,(short)z,type);
					break;
				case DrawMode.Hollow:
					for (int x=x1;x<x2;x++)
						for (int y=y1;y<y2;y++) {
						SetBlock(player,(short)x,(short)y,(short)z1,type);
						SetBlock(player,(short)x,(short)y,(short)(z2-1),type);
					} for (int y=y1;y<y2;y++)
						for (int z=z1+1;z<z2-1;z++) {
						SetBlock(player,(short)x1,(short)y,(short)z,type);
						SetBlock(player,(short)(x2-1),(short)y,(short)z,type);
					} for (int x=x1+1;x<x2-1;x++)
						for (int z=z1+1;z<z2-1;z++) {
						SetBlock(player,(short)x,(short)y1,(short)z,type);
						SetBlock(player,(short)x,(short)(y2-1),(short)z,type);
					} break;
				case DrawMode.Wireframe:
					for (int x=x1;x<x2;x++) {
						SetBlock(player,(short)x,(short)y1,(short)z1,type);
						SetBlock(player,(short)x,(short)y1,(short)(z2-1),type);
						SetBlock(player,(short)x,(short)(y2-1),(short)z1,type);
						SetBlock(player,(short)x,(short)(y2-1),(short)(z2-1),type);
					} for (int y=y1+1;y<y2-1;y++) {
						SetBlock(player,(short)x1,(short)y,(short)z1,type);
						SetBlock(player,(short)x1,(short)y,(short)(z2-1),type);
						SetBlock(player,(short)(x2-1),(short)y,(short)z1,type);
						SetBlock(player,(short)(x2-1),(short)y,(short)(z2-1),type);
					} for (int z=z1+1;z<z2-1;z++) {
						SetBlock(player,(short)x1,(short)y1,(short)z,type);
						SetBlock(player,(short)x1,(short)(y2-1),(short)z,type);
						SetBlock(player,(short)(x2-1),(short)y1,(short)z,type);
						SetBlock(player,(short)(x2-1),(short)(y2-1),(short)z,type);
					} break;
			}
			
		}
		public void Sphere(Player player,int x,int y,int z,int radius,byte type) {
			Sphere(player,x,y,z,radius,type,DrawMode.Solid);
		}
		public void Sphere(Player player,int x,int y,int z,int radius,byte type,DrawMode mode) {
			switch (mode) {
				case DrawMode.Solid:
					for (int xx=-radius;xx<radius;xx++)
						for (int yy=-radius;yy<radius;yy++)
							for (int zz=-radius;zz<radius;zz++)
								if (Math.Sqrt(Math.Pow(xx,2)+Math.Pow(yy,2)+Math.Pow(zz,2))+0.5<=radius &&
								    x+xx>=0 && y+yy>=0 && z+zz>=0 && x+xx<width && y+yy<depth && z+zz<height)
									SetBlock(player,(short)(xx+x),(short)(yy+y),(short)(zz+z),type);
					break;
				case DrawMode.Hollow:
					SphereHollow(player,x,y,z,radius,radius-1,type);
					break;
				case DrawMode.Wireframe:
					throw new NotImplementedException();
			}
		}
		public void SphereHollow(Player player,int x,int y,int z,int radius,int hollow,byte type) {
			for (int xx=-radius;xx<radius;xx++)
				for (int yy=-radius;yy<radius;yy++)
					for (int zz=-radius;zz<radius;zz++) {
				double distance = Math.Sqrt(Math.Pow(xx,2)+Math.Pow(yy,2)+Math.Pow(zz,2))+0.5;
				if (distance<=radius && distance>hollow &&
				    x+xx>=0 && y+yy>=0 && z+zz>=0 && x+xx<width && y+yy<depth && z+zz<height)
					SetBlock(player,(short)(xx+x),(short)(yy+y),(short)(zz+z),type);
			}
		}
		public enum DrawMode {
			Solid,
			Hollow,
			Wireframe
		}
		#endregion
		
		public static Level Load(string name) {
			if (name==null) { throw new ArgumentNullException("name"); }
			if (name=="") { throw new ArgumentException("Name musn't be an empty string.","name"); }
			if (!RegexHelper.IsAlphaNumeric(name)) {
				throw new ArgumentException("Only alphanumerical characters allowed.","name");
			} string filename = "levels/"+name+".lvl";
			try {
				Node node = host.Load(filename,out name);
				if (name!="obsidian-level") { return null; }
				short width = (short)node["width"].Value;
				short depth = (short)node["depth"].Value;
				short height = (short)node["height"].Value;
				Node spawnNode = node["spawn"];
				Position spawn = new Position(
					(short)spawnNode["x"].Value,(short)spawnNode["y"].Value,(short)spawnNode["z"].Value,
					(byte)spawnNode["rotx"].Value,(byte)spawnNode["roty"].Value);
				byte[] mapdata = (byte[])node["mapdata"].Value;
				byte[] blockdata = (byte[])node["blockdata"].Value;
				Level level = new Level(width,depth,height);
				level.spawn.Set(spawn);
				level.mapdata = mapdata;
				level.blockdata = blockdata;
				level.custom = node["custom"]??level.custom;
				return level;
			} catch { return null; }
		}
		public void Save(string name) {
			if (name==null) { throw new ArgumentNullException("name"); }
			if (name=="") { throw new ArgumentException("Name musn't be an empty string.","name"); }
			if (!RegexHelper.IsAlphaNumeric(name)) {
				throw new ArgumentException("Only alphanumerical characters allowed.","name");
			} Node node = new Node();
			node["width"] = new Node(width);
			node["depth"] = new Node(depth);
			node["height"] = new Node(height);
			Node spawnNode = new Node();
			node["spawn"] = spawnNode;
			spawnNode["x"] = new Node((short)spawn.X);
			spawnNode["y"] = new Node((short)spawn.Y);
			spawnNode["z"] = new Node((short)spawn.Z);
			spawnNode["rotx"] = new Node(spawn.RotX);
			spawnNode["roty"] = new Node(spawn.RotY);
			node["mapdata"] = new Node(mapdata);
			node["blockdata"] = new Node(blockdata);
			node["custom"] = custom;
			Saving.Raise(server,this);
			if (!Directory.Exists("levels")) { Directory.CreateDirectory("levels"); }
			host.Save("levels/"+name+".lvl",node,"obsidian-level");
		}
	}
}
