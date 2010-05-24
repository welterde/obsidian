using System;
using System.Collections.Generic;

namespace obsidian.World {
	public class Blocktype {
		#region Members
		private readonly byte id;
		private readonly string name;
		private readonly bool placeable;
		private readonly bool solid;
		private readonly bool opaque;
		#endregion
		
		#region Public members
		public byte ID {
			get { return id; }
		}
		public string Name {
			get { return name; }
		}
		public bool Placeable {
			get { return placeable; }
		}
		public bool Solid {
			get { return solid; }
		}
		public bool Opaque {
			get { return opaque; }
		}
		#endregion
		
		private Blocktype(byte id,string name,bool placeable,bool solid,bool opaque) {
			this.id = id;
			this.name = name;
			this.placeable = placeable;
			this.solid = solid;
			this.opaque = opaque;
		}
		
		private static bool initialized = false;
		private static List<Blocktype> blocktypes = new List<Blocktype>(50);
		
		public static Blocktype air,rock,grass,dirt,cobblestone,wood,sapling,
		              adminium,water,stillWater,lava,stillLava,sand,gravel,
		              goldOre,ironOre,coalOre,tree,leaves,sponge,glass,red,
		              orange,yellow,lightGreen,green,aquaGreen,cyan,blue,
		              purple,indigo,violet,magenta,pink,black,gray,white,flower,
		              rose,brownMushroom,redMushroom,gold,iron,doubleStair,
		              stair,redBrick,TNT,bookcase,mossyCobblestone,obsidian;
		public static byte Count {
			get { return (byte)blocktypes.Count; }
		}
		
		public static void Init() {
			if (initialized) { throw new Exception("Blocktypes are already initialized!"); }
			initialized = true;
			air = Add("Air",false,false,false);
			rock = Add("Rock");
			grass = Add("Grass",false);
			dirt = Add("Dirt");
			cobblestone = Add("Cobblestone");
			wood = Add("Wood");
			sapling = Add("Sapling",true);
			adminium = Add("Adminium",false);
			water = Add("Water",false,false);
			stillWater = Add("StillWater",false,false);
			lava = Add("Lava",false,false);
			stillLava = Add("StillLava",false,false);
			sand = Add("Sand");
			gravel = Add("Gravel");
			goldOre = Add("GoldOre");
			ironOre = Add("IronOre");
			coalOre = Add("CoalOre");
			tree = Add("Tree");
			leaves = Add("Leaves");
			sponge = Add("Sponge");
			glass = Add("Glass",true,true,false);
			red = Add("Red");
			orange = Add("Orange");
			yellow = Add("Yellow");
			lightGreen = Add("LightGreen");
			green = Add("Green");
			aquaGreen = Add("AquaGreen");
			cyan = Add("Cyan");
			blue = Add("Blue");
			purple = Add("Purple");
			indigo = Add("Indigo");
			violet = Add("Violet");
			magenta = Add("Magenta");
			pink = Add("Pink");
			black = Add("Black");
			gray = Add("Gray");
			white = Add("White");
			flower = Add("Flower",true);
			rose = Add("Rose",true);
			brownMushroom = Add("BrownMushroom",true);
			redMushroom = Add("RedMushroom",true);
			gold = Add("Gold");
			iron = Add("Iron");
			doubleStair = Add("DoubleStair",false);
			stair = Add("Stair",true,false,true);
			redBrick = Add("RedBrick");
			TNT = Add("TNT");
			bookcase = Add("Bookcase");
			mossyCobblestone = Add("MossyCobblestone");
			obsidian = Add("Obsidian");
		}
		
		private static Blocktype Add(string name) {
			return Add(name,true,true,true);
		}
		private static Blocktype Add(string name,bool placeable) {
			return Add(name,placeable,!placeable,!placeable);
		}
		private static Blocktype Add(string name,bool placeable,bool solid) {
			return Add(name,placeable,solid,solid);
		}
		private static Blocktype Add(string name,bool placeable,bool solid,bool opaque) {
			Blocktype type = new Blocktype(Count,name,placeable,solid,opaque);
			blocktypes.Add(type);
			return type;
		}
		
		public static Blocktype FindById(byte id) {
			if (id>Count) { return null; }
			return blocktypes[id];
		}
		public static Blocktype FindByName(string name) {
			return blocktypes.Find(
				delegate(Blocktype blocktype) {
					return blocktype.Name.Equals(name,StringComparison.OrdinalIgnoreCase);
				});
		}
	}
}
