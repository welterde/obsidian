using System;
using System.Collections.Generic;
using NamedBinaryTag;

namespace obsidian.Data {
	internal class NbtHost : IDataHost {
		private Type[] supported = new Type[10]{
			typeof(byte),typeof(short),typeof(int),typeof(long),typeof(float),typeof(double),
			typeof(byte[]),typeof(string),typeof(Node.List),typeof(Node.Compound)};
		private string[] names = new string[10]{
			"byte","short","int","long","float","double",
			"byte[]","string","list","compound"};
		private Type[] tagType = new Type[10]{
			typeof(TagByte),typeof(TagShort),typeof(TagInt),typeof(TagLong),typeof(TagFloat),typeof(double),
			typeof(TagByteArray),typeof(TagString),typeof(TagList),typeof(TagCompound)};
		
		public string Extension {
			get { return "nbt"; }
		}
		public bool Supports(Type type) {
			return ((IList<Type>)supported).Contains(type);
		}
		
		public Node Load(string filename,out string name) {
			TagCompound compound;
			try {
				compound = new TagCompound();
				compound.Load(filename,out name);
			} catch (Exception e) { throw new FormatException("Could not load or parse '"+filename+"'.",e); }
			return TagToNode(compound);
		}
		public void Save(string filename,Node root,string name) {
			if (!root.IsCompound()) { throw new FormatException("Root node isn't compound."); }
			((TagCompound)NodeToTag(root)).Save(filename,name);
		}
		
		private Node TagToNode(Tag tag) {
			Node node = null;
			switch (tag.GetId()) {
				case 1: node = (byte)tag; break;
				case 2: node = (short)tag; break;
				case 3: node = (int)tag; break;
				case 4: node = (long)tag; break;
				case 5: node = (float)tag; break;
				case 6: node = (double)tag; break;
				case 7: node = (byte[])tag; break;
				case 8: node = (string)tag; break;
				case 9:
					node = new Node.List();
					foreach (Tag t in (TagList)tag) {
						node += TagToNode(t);
					} break;
				case 10:
					node = new Node.Compound();
					foreach (KeyValuePair<string,Tag> kvp in (TagCompound)tag) {
						node[kvp.Key] = TagToNode(kvp.Value);
					} break;
			} return node;
		}
		private Tag NodeToTag(Node node) {
			if (!Supports(node.Type)) { throw new FormatException("NbtHost doesn't support "+node.Type.ToString()+"."); }
			Tag tag = null;
			switch (((IList<Type>)supported).IndexOf(node.Type)) {
				case 0: tag = (TagByte)(byte)node; break;
				case 1: tag = (TagShort)(short)node; break;
				case 2: tag = (TagInt)(int)node; break;
				case 3: tag = (TagLong)(long)node; break;
				case 4: tag = (TagFloat)(float)node; break;
				case 5: tag = (TagDouble)(double)node; break;
				case 6: tag = (TagByteArray)(byte[])node; break;
				case 7: tag = (TagString)(string)node; break;
				case 8:
					TagList list = new TagList(ListTagType(node)??typeof(TagByte));
					try {
						foreach (Node n in (Node.List)node) {
							if (n.IsNull()) { continue; }
							list.Add(NodeToTag(n));
						}
					} catch { throw new FormatException("List musn't contain different node types."); }
					tag = list;
					break;
				case 9:
					TagCompound compound = new TagCompound();
					foreach (KeyValuePair<string,Node> kvp in (Node.Compound)node) {
						compound.Add(kvp.Key,NodeToTag(kvp.Value));
					} tag = compound;
					break;
			} return tag;
		}
		
		private Type ListTagType(Node node) {
			if (node.Count==0) { return null; }
			else { return tagType[((IList<Type>)supported).IndexOf(node[0].Type)]; }
		}
	}
}
