using System;
using System.Collections;
using System.Collections.Generic;

namespace obsidian.Data {
	public class Node : IEnumerable<Node> {
		private object value;
		
		public Type Type {
			get { return value.GetType(); }
		}
		public int Count {
			get {
				if (IsList()) { return AsList().Count; }
				else if (IsCompound()) { return AsCompound().Count; }
				else { return -1; }
			}
		}
		public Node this[int index] {
			get { return AsList()[index]; }
			set { AsList()[index] = value; }
		}
		public Node this[string key] {
			get {
				Compound c = AsCompound();
				if (c.ContainsKey(key)) { return c[key]; }
				else {
					Node node = new Node();
					this[key] = node;
					return node;
				}
			}
			set {
				if (IsNull()) { this.value = new Compound(); }
				Compound c = AsCompound();
				if (c.ContainsKey(key)) { c[key] = value; }
				else { c.Add(key,value); }
			}
		}
		
		private Node() : this(null) {  }
		private Node(object value) { this.value = value; }
		
		public void Remove(Node node) { AsList().Remove(node); }
		public void RemoveAt(int index) { AsList().RemoveAt(index); }
		public void Remove(string key) { AsCompound().Remove(key); }
		
		private T As<T>() {
			if (!Is<T>()) { throw new InvalidCastException("Trying to cast from "+value.GetType()+" to "+typeof(T)+"."); }
			return (T)value;
		}
		private bool AsBool() { return As<bool>(); }
		private byte AsByte() { return As<byte>(); }
		private short AsShort() { return As<short>(); }
		private int AsInt() { return As<int>(); }
		private long AsLong() { return As<long>(); }
		private float AsFloat() { return As<float>(); }
		private double AsDouble() { return As<double>(); }
		private byte[] AsBytes() { return As<byte[]>(); }
		private string AsString() { return As<string>(); }
		private List AsList() { return As<List>(); }
		private Compound AsCompound() { return As<Compound>(); }
		
		private bool Is<T>() { return (value is T); }
		public bool IsBool() { return Is<bool>(); }
		public bool IsByte() { return Is<byte>(); }
		public bool IsShort() { return Is<short>(); }
		public bool IsInt() { return Is<int>(); }
		public bool IsLong() { return Is<long>(); }
		public bool IsFloat() { return Is<float>(); }
		public bool IsDouble() { return Is<double>(); }
		public bool IsBytes() { return Is<byte[]>(); }
		public bool IsString() { return Is<string>(); }
		public bool IsList() { return Is<List>(); }
		public bool IsCompound() { return Is<Compound>(); }
		public bool IsNull() { return (value==null); }
		
		public IEnumerator<Node> GetEnumerator() { return AsList().GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public override string ToString() { return value.ToString(); }
		
		public static Node operator +(Node left,Node right) {
			if (left.IsNull()) { left.value = new List(); }
			left.AsList().Add(right); return left;
		}
		
		public static explicit operator bool(Node node) { return node.AsBool(); }
		public static explicit operator byte(Node node) { return node.AsByte(); }
		public static explicit operator short(Node node) { return node.AsShort(); }
		public static explicit operator int(Node node) { return node.AsInt(); }
		public static explicit operator long(Node node) { return node.AsLong(); }
		public static explicit operator float(Node node) { return node.AsFloat(); }
		public static explicit operator double(Node node) { return node.AsDouble(); }
		public static explicit operator byte[](Node node) { return node.AsBytes(); }
		public static explicit operator string(Node node) { return node.AsString(); }
		public static explicit operator List(Node node) { return node.AsList(); }
		public static explicit operator Compound(Node node) { return node.AsCompound(); }
		
		public static implicit operator Node(bool value) { return new Node(value); }
		public static implicit operator Node(byte value) { return new Node(value); }
		public static implicit operator Node(short value) { return new Node(value); }
		public static implicit operator Node(int value) { return new Node(value); }
		public static implicit operator Node(long value) { return new Node(value); }
		public static implicit operator Node(float value) { return new Node(value); }
		public static implicit operator Node(double value) { return new Node(value); }
		public static implicit operator Node(byte[] value) { return new Node(value); }
		public static implicit operator Node(string value) { return new Node(value); }
		public static implicit operator Node(List value) { return new Node(value); }
		public static implicit operator Node(Compound value) { return new Node(value); }
		
		public class List : List<Node> {  }
		public class Compound : Dictionary<string,Node> {  }
	}
}
