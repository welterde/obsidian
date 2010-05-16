using System;
using System.Collections.Generic;

namespace obsidian.Data {
	public abstract class DataHost {
		protected abstract Type[] Supported { get; }
		
		public virtual string Extension {
			get { return "dat"; }
		}
		
		public bool Supports(Type type) {
			if (type==null) { throw new ArgumentNullException("type"); }
			return ((IList<Type>)Supported).Contains(type);
		}
		public bool Supports(object obj) {
			if (obj==null) { throw new ArgumentNullException("obj"); }
			return Supports(obj.GetType());
		}
		public bool Supports<T>() {
			return Supports(typeof(T));
		}
		
		public bool IsValid(Node node) {
			if (!(node.Value is Dictionary<string,Node>)) { return false; }
			try { Check(node,new LinkedList<Node>()); }
			catch { return false; }
			return true;
		}
		protected void Check(Node node,LinkedList<Node> list) {
			if (node.Value==null) { return; }
			if (list.Contains(node)) { throw new Exception("Two or more nodes share the same reference."); }
			list.AddLast(node);
			if (!Supports(node.Value)) { throw new Exception(node.Value.GetType()+" isn't supported."); }
			CheckNode(node);
			List<Node> l = node.Value as List<Node>;
			if (l!=null) { foreach (Node n in l) { Check(n,list); } return; }
			Dictionary<string,Node> d = node.Value as Dictionary<string,Node>;
			if (d!=null) foreach (KeyValuePair<string,Node> kvp in d) {
				CheckName(kvp.Key);
				Check(kvp.Value,list);
			}
		}
		protected abstract void CheckName(string name);
		protected abstract void CheckNode(Node node);
		
		public abstract Node Load(string filename,out string name);
		public abstract void Save(string filename,Node root,string name);
	}
}
