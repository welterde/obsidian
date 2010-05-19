using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace obsidian.Data {
	public class XmlHost : DataHost {
		private static XmlHost instance = new XmlHost();
		private static Type[] supported = new Type[10]{
			typeof(bool),typeof(byte),typeof(short),typeof(int),typeof(long),typeof(float),
			typeof(double),typeof(string),typeof(List<Node>),typeof(Dictionary<string,Node>) };
		private static string[] names = new string[10]{
			"bool","byte","short","int","long","float","double","string","list","compound" };
		
		public static XmlHost Instance {
			get { return instance; }
		}
		
		protected override Type[] Supported {
			get { return supported; }
		}
		public override string Extension {
			get { return "xml"; }
		}
		
		private XmlHost() {  }
		
		protected override void CheckName(string name) {  }
		protected override void CheckNode(Node node) {  }
		
		public override Node Load(string filename,out string name) {
			if (filename==null) { throw new ArgumentNullException("filename"); }
			XElement root = XElement.Load(filename);
			name = GetElementName(root);
			return ElementToNode(root);
		}
		public override void Save(string filename,Node root,string name) {
			if (filename==null) { throw new ArgumentNullException("filename"); }
			if (root==null) { throw new ArgumentNullException("root"); }
			if (name==null) { throw new ArgumentNullException("name"); }
			Check(root,new LinkedList<Node>());
			XElement element = NodeToElement(root);
			element.SetAttributeValue("name",name);
			XmlWriterSettings settings = new XmlWriterSettings(){ OmitXmlDeclaration=false };
			using (XmlWriter writer = XmlWriter.Create(filename,settings)) {
				element.Save(writer);
			}
		}
		
		private string GetElementName(XElement element) {
			XAttribute attr = element.Attribute("name");
			if (attr==null) { throw new FormatException("There is no name attribute."); }
			return attr.Value;
		}
		
		private Node ElementToNode(XElement element) {
			Node node = new Node();
			switch (element.Name.LocalName) {
					case "bool": node.Value = bool.Parse(element.Value); break;
					case "byte": node.Value = byte.Parse(element.Value); break;
					case "short": node.Value = short.Parse(element.Value); break;
					case "int": node.Value = int.Parse(element.Value); break;
					case "long": node.Value = long.Parse(element.Value); break;
					case "float": node.Value = float.Parse(element.Value); break;
					case "double": node.Value = double.Parse(element.Value); break;
					case "string": node.Value = element.Value; break;
				case "list":
					foreach (XElement e in element.Elements())
						node.Add(ElementToNode(e));
					break;
				case "compound":
					foreach (XElement e in element.Elements())
						node[GetElementName(e)] = ElementToNode(e);
					break;
				default:
					throw new FormatException("Can't parse '"+element.Name.LocalName+"'.");
			} return node;
		}
		
		private XElement NodeToElement(Node node) {
			int index = ((IList<Type>)supported).IndexOf(node.Value.GetType());
			switch (index) {
				case 8:
					XElement list = new XElement("list");
					foreach (Node n in (List<Node>)node.Value) {
						if (n.Value==null) { continue; }
						list.Add(NodeToElement(n));
					} return list;
				case 9:
					XElement compound = new XElement("compound");
					foreach (KeyValuePair<string,Node> kvp in (Dictionary<string,Node>)node.Value) {
						if (kvp.Value.Value==null) { continue; }
						XElement e = NodeToElement(kvp.Value);
						e.SetAttributeValue("name",kvp.Key);
						compound.Add(e);
					} return compound;
					default: return new XElement(names[index],node.Value);
			}
		}
	}
}
