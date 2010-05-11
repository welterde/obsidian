using System;
using System.Xml;
using System.Collections.Generic;

namespace obsidian.Data {
	// TODO: Rewrite this. Use XmlReader/Writer.
	// TODO: Support byte arrays, saved as hexadecimal string.
	internal class XmlHost : IDataHost {
		private Type[] supported = new Type[9]{
			typeof(byte),typeof(short),typeof(int),typeof(long),typeof(float),
			typeof(double),typeof(string),typeof(Node.List),typeof(Node.Compound)};
		private string[] names = new string[9]{
			"byte","short","int","long","float",
			"double","string","list","compound"};
		
		public string Extension {
			get { return "xml"; }
		}
		public bool Supports(Type type) {
			return ((IList<Type>)supported).Contains(type);
		}
		
		public Node Load(string filename,out string name) {
			XmlDocument doc = new XmlDocument();
			try { doc.Load(filename); }
			catch (Exception e) { throw new FormatException("Could not load or parse '"+filename+"'.",e); }
			XmlNode root = doc.FirstChild;
			if (root is XmlElement && root.NextSibling==null) {
				return XmlElementToNode((XmlElement)root,out name);
			} else { throw new FormatException("Invalid format of XML file."); }
		}
		public void Save(string filename,Node root,string name) {
			XmlDocument doc = new XmlDocument();
			doc.AppendChild(NodeToXmlElement(doc,root,name));
			doc.Save(filename);
		}
		
		private Node XmlElementToNode(XmlElement element,out string name) {
			int index = ((IList<string>)names).IndexOf(element.Name);
			if (index<0) { throw new FormatException("Unknown type '"+element.Name+"'."); }
			Node node = null;
			try {
				if (element.NodeType!=XmlNodeType.Element) { throw new FormatException("XmlElement expected."); }
				switch (element.Name) {
					case "byte": node = byte.Parse(element.InnerText); break;
					case "short": node = short.Parse(element.InnerText); break;
					case "int": node = int.Parse(element.InnerText); break;
					case "long": node = long.Parse(element.InnerText); break;
					case "float": node = float.Parse(element.InnerText); break;
					case "double": node = double.Parse(element.InnerText); break;
					case "string": node = element.InnerText; break;
					case "list":
						if (element.Name!="list") { throw new FormatException("List expected."); }
						node = new Node.List();
						foreach (XmlNode n in element.ChildNodes) {
							if (n is XmlElement) {
								string s;
								node += XmlElementToNode((XmlElement)n,out s);
							} else { throw new FormatException("XmlElement expected."); }
						} break;
					case "compound":
						if (element.Name!="compound") { throw new FormatException("Compound expected."); }
						node = new Node.Compound();
						foreach (XmlNode n in element.ChildNodes) {
							if (n is XmlElement) {
								string s;
								Node child = XmlElementToNode((XmlElement)n,out s);
								if (s==null) { throw new FormatException("Name attribute required."); }
								node[s] = child;
							} else { throw new FormatException("XmlElement expected."); }
						} break;
				}
			} catch (FormatException e) { throw e; }
			catch (Exception e) { throw new FormatException("Could not parse value to "+element.Name+".",e); }
			if (element.HasAttribute("name")) { name = element.GetAttribute("name"); }
			else { name = null; }
			return node;
		}
		private XmlElement NodeToXmlElement(XmlDocument doc,Node node,string name) {
			if (!Supports(node.Type)) { throw new FormatException("XmlHost doesn't support "+node.Type+"."); }
			string type = names[((IList<Type>)supported).IndexOf(node.Type)];
			XmlElement element = doc.CreateElement(type);
			if (name!=null) { element.SetAttribute("name",name); }
			switch (type) {
				case "list":
					if (node.Count!=0) {
						foreach (Node n in (Node.List)node) {
							element.AppendChild(NodeToXmlElement(doc,n,null));
						}
					} break;
				case "compound":
					foreach (KeyValuePair<string,Node> kvp in (Node.Compound)node)
						element.AppendChild(NodeToXmlElement(doc,kvp.Value,kvp.Key));
					break;
					default: element.InnerText = node.ToString(); break;
			} return element;
		}
	}
}
