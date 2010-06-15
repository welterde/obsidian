using System;
using System.Collections.Generic;
using NBT;

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
			Tag root = Tag.Load(filename);
			name = root.Name;
			return TagToNode(root);
		}
		public override void Save(string filename,Node root,string name) {
			if (filename==null) { throw new ArgumentNullException("filename"); }
			if (root==null) { throw new ArgumentNullException("root"); }
			if (name==null) { throw new ArgumentNullException("name"); }
			Tag tag = Tag.Create(name);
			foreach (KeyValuePair<string,Node> kvp in (Dictionary<string,Node>)root.Value)
				AddNode(tag,kvp.Key,kvp.Value);
			tag.Save(filename);
		}
		
		private Node TagToNode(Tag tag) {
			Node node = new Node();
			switch (tag.Type) {
				case TagType.Byte: node.Value = (byte)tag; break;
				case TagType.Short: node.Value = (short)tag; break;
				case TagType.Int: node.Value = (int)tag; break;
				case TagType.Long: node.Value = (long)tag; break;
				case TagType.Float: node.Value = (float)tag; break;
				case TagType.Double: node.Value = (double)tag; break;
				case TagType.Byte_Array: node.Value = (byte[])tag; break;
				case TagType.String: node.Value = (string)tag; break;
				case TagType.List:
					foreach (Tag t in tag) node.Add(TagToNode(t));
					break;
				case TagType.Compound:
					foreach (Tag t in tag) node[t.Name] = TagToNode(t);
					break;
			} return node;
		}
		
		private void AddNode(Tag parent,Node node) {
			AddNode(parent,null,node);
		}
		private void AddNode(Tag parent,string name,Node node) {
			switch (((IList<Type>)supported).IndexOf(node.Value.GetType())) {
				case 8:
					List<Node> l = (List<Node>)node.Value;
					Tag list = parent.AddList(ListType(l));
					foreach (Node n in l) AddNode(list,n);
					break;
				case 9:
					Dictionary<string,Node> dict = (Dictionary<string,Node>)node.Value;
					Tag compound = parent.AddCompound();
					foreach (KeyValuePair<string,Node> kvp in dict)
						AddNode(parent,kvp.Key,kvp.Value);
					break;
				default: parent.Add(node.Value); break;
			}
		}
		
		private TagType ListType(List<Node> list) {
			Type type = null;
			foreach (Node n in list) {
				if (n.Value==null) { continue; }
				Type t = n.Value.GetType();
				if (type==null) { type = t; continue; }
				if (type!=t) { throw new Exception("List<Node> musn't contain different types of values."); }
			} return (TagType)((IList<Type>)supported).IndexOf(type);
		}
	}
}
