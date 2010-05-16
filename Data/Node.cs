using System;
using System.Collections;
using System.Collections.Generic;

namespace obsidian.Data {
	public class Node {
		private object value;
		
		public object Value {
			get { return value; }
			set { this.value = value; }
		}
		public int Count {
			get { return GetCount(); }
		}
		public Node this[object index] {
			get { return GetIndex(index); }
			set { SetIndex(index,value); }
		}
		
		public Node() : this(null) {  }
		public Node(object value) {
			this.value = value;
		}
		
		public void Add(Node value) {
			if (value==null) { throw new ArgumentNullException("node"); }
			if (this.value==null) { this.value = new List<Node>(); }
			List<Node> list = this.value as List<Node>;
			if (list==null) { throw new Exception("Value isn't a List<Node>."); }
			list.Add(value);
		}
		public bool Contains(object value) {
			List<Node> list = this.value as List<Node>;
			if (list==null) { throw new Exception("Value isn't a List<Node>."); }
			foreach (Node node in list) {
				if (node.value==null) {
					if (value==null) { return true; }
					continue;
				} if (node.value.Equals(value)) { return true; }
			} return false;
		}
		
		public void ListForeach(Action<Node> action) {
			List<Node> list = this.value as List<Node>;
			if (list==null) { throw new Exception("Value isn't a List<Node>."); }
			foreach (Node node in list) { action(node); }
		}
		public void DictForeach(Action<string,Node> action) {
			Dictionary<string,Node> dict = this.value as Dictionary<string,Node>;
			if (dict==null) { throw new Exception("Value isn't a Dictionary<string,Node>."); }
			foreach (KeyValuePair<string,Node> kvp in dict) { action(kvp.Key,kvp.Value); }
		}
		
		private Node GetIndex(object index) {
			if (index==null) { throw new ArgumentNullException("index"); }
			if (index is string) { return DictGet((string)index); }
			if (index is IConvertible) { return ListGet(Convert.ToInt32(index)); }
			throw new ArgumentException("Indexer isn't string or int compatible.","index");
		}
		private void SetIndex(object index,Node value) {
			if (index==null) { throw new ArgumentNullException("index"); }
			if (value==null) { throw new ArgumentNullException("value"); }
			if (index is string) { DictSet((string)index,value); return; }
			if (index is IConvertible) { ListSet(Convert.ToInt32(index),value); return; }
			throw new ArgumentException("Indexer isn't string or int compatible.","index");
		}
		
		private int GetCount() {
			ICollection coll = (value as ICollection);
			if (coll==null) { return -1; }
			return coll.Count;
		}
		
		private Node DictGet(string key) {
			if (value==null) { return null; }
			Dictionary<string,Node> dict = this.value as Dictionary<string,Node>;
			if (dict==null) { throw new Exception("Value isn't a Dictionary<string,Node>."); }
			if (!dict.ContainsKey(key)) { return null; }
			return dict[key];
		}
		private void DictSet(string key,Node value) {
			if (this.value==null) { this.value = new Dictionary<string,Node>(StringComparer.OrdinalIgnoreCase); }
			Dictionary<string,Node> dict = this.value as Dictionary<string,Node>;
			if (dict==null) { throw new Exception("Value isn't a Dictionary<string,Node>."); }
			if (!dict.ContainsKey(key)) { dict.Add(key,value); }
			else { dict[key] = value; }
		}
		
		private Node ListGet(int index) {
			List<Node> list = value as List<Node>;
			if (list==null) { throw new Exception("Value isn't a List<Node>."); }
			if (index<0 || index>=list.Count) { throw new ArgumentOutOfRangeException("index"); }
			return list[index];
		}
		private void ListSet(int index,Node value) {
			if (this.value==null && index==0) { this.value = new List<Node>(); }
			List<Node> list = this.value as List<Node>;
			if (list==null) { throw new Exception("Value isn't a List<Node>."); }
			if (index<0 || index>=list.Count) { throw new ArgumentOutOfRangeException("index"); }
			else { list[index] = value; }
		}
	}
}
