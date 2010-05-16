using System;
using System.Collections.Generic;
using NamedBinaryTag;

namespace obsidian.Data {
	public class NbtHost : DataHost {
		private static NbtHost instance = new NbtHost();
		private static Type[] supported = new Type[10]{
			typeof(byte),typeof(short),typeof(int),typeof(long),typeof(float),typeof(double),
			typeof(byte[]),typeof(string),typeof(List<Node>),typeof(Dictionary<string,Node>) };
		
		public static NbtHost Instance {
			get { return instance; }
		}
		
		protected override Type[] Supported {
			get { return supported; }
		}
		public override string Extension {
			get { return "nbt"; }
		}
		
		private NbtHost() {  }
		
		protected override void CheckName(string name) {  }
		protected override void CheckNode(Node node) {
			List<Node> list = node.Value as List<Node>;
			if (list!=null) { ListType(list); }
		}
		
		public override Node Load(string filename,out string name) {
			if (filename==null) { throw new ArgumentNullException("filename"); }
			TagCompound root = new TagCompound();
			root.Load(filename,out name);
			return TagToNode(root);
		}
		public override void Save(string filename,Node root,string name) {
			if (filename==null) { throw new ArgumentNullException("filename"); }
			if (root==null) { throw new ArgumentNullException("root"); }
			if (name==null) { throw new ArgumentNullException("name"); }
			Check(root,new LinkedList<Node>());
			((TagCompound)NodeToTag(root)).Save(filename,name);
		}
		
		private Node TagToNode(Tag tag) {
			Node node = new Node();
			switch (tag.GetId()) {
				case 1: node.Value = (byte)tag; break;
				case 2: node.Value = (short)tag; break;
				case 3: node.Value = (int)tag; break;
				case 4: node.Value = (long)tag; break;
				case 5: node.Value = (float)tag; break;
				case 6: node.Value = (double)tag; break;
				case 7: node.Value = (byte[])tag; break;
				case 8: node.Value = (string)tag; break;
				case 9:
					foreach (Tag t in (TagList)tag)
						node.Add(TagToNode(t));
					break;
				case 10:
					foreach (KeyValuePair<string,Tag> kvp in (TagCompound)tag)
						node[kvp.Key] = TagToNode(kvp.Value);
					break;
			} return node;
		}
		
		private Tag NodeToTag(Node node) {
			switch (((IList<Type>)supported).IndexOf(node.Value.GetType())) {
				case 0: return new TagByte((byte)node.Value);
				case 1: return new TagShort((short)node.Value);
				case 2: return new TagInt((int)node.Value);
				case 3: return new TagLong((long)node.Value);
				case 4: return new TagFloat((float)node.Value);
				case 5: return new TagDouble((double)node.Value);
				case 6: return new TagByteArray((byte[])node.Value);
				case 7: return new TagString((string)node.Value);
				case 8:
					List<Node> l = (List<Node>)node.Value;
					TagList list = new TagList(ListType(l)??typeof(TagByte));
					foreach (Node n in l) {
						if (n.Value==null) { continue; }
						list.Add(NodeToTag(n));
					} return list;
				case 9:
					TagCompound compound = new TagCompound();
					foreach (KeyValuePair<string,Node> kvp in (Dictionary<string,Node>)node.Value) {
						if (kvp.Value.Value==null) { continue; }
						compound.Add(kvp.Key,NodeToTag(kvp.Value));
					} return compound;
			} return null;
		}
		
		private Type ListType(List<Node> list) {
			Type type = null;
			foreach (Node n in list) {
				if (n.Value==null) { continue; }
				Type t = n.Value.GetType();
				t = Tag.FromId((byte)(((IList<Type>)supported).IndexOf(t)+1));
				if (type==null) { type = t; continue; }
				if (type!=t) { throw new Exception("List<Node> musn't contain different types of values."); }
			} return type;
		}
	}
}
