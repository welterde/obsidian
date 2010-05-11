using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using obsidian.World;
using obsidian.Data;

namespace obsidian.Control {
	public class Group {
		private static IDataHost host = new XmlHost();
		
		#region Members
		private readonly string name;
		private bool standard = false;
		private string prefix = "";
		private string postfix = "";
		private List<Command> commands = new List<Command>();
		#endregion
		
		#region Public members
		public string Name {
			get { return name; }
		}
		public bool Standard {
			get { return standard; }
		}
		public string Prefix {
			get { return prefix; }
			set { prefix = value; }
		}
		public string Postfix {
			get { return postfix; }
			set { postfix = value; }
		}
		public List<Command> Commands {
			get { return commands; }
		}
		#endregion
		
		private Group(string name) {
			this.name = name;
		}
		
		private static Group Load(Command.List commands,string name) {
			try {
				string filename = "groups/"+name+"."+host.Extension;
				string rootName;
				Node node = host.Load(filename,out rootName);
				if (rootName!="group") { return null; }
				string n = (string)node["name"];
				if (n.ToLower()!=name.ToLower()) { return null; }
				Group group = new Group(n);
				group.standard = bool.Parse((string)node["standard"]);
				group.prefix = (string)node["prefix"];
				group.postfix = (string)node["postfix"];
				group.commands = commands.LoadPermissionList(node["commands"]);
				return group;
			} catch { return null; }
		}
		public void Save() {
			Node node = new Node.Compound();
			node["name"] = name;
			node["standard"] = standard.ToString();
			node["prefix"] = prefix;
			node["postfix"] = postfix;
			node["commands"] = Command.List.SavePermissionList(commands);
			if (!Directory.Exists("groups")) { Directory.CreateDirectory("groups"); }
			host.Save("groups/"+name+"."+host.Extension,node,"group");
		}
		
		public class List {
			private Dictionary<string,Group> groups = new Dictionary<string,Group>();
			private Group standard = null;
			
			internal List() {  }
			
			public Group Standard {
				get { return standard; }
			}
			public Group this[string name] {
				get { return groups.ContainsKey(name.ToLower())?groups[name.ToLower()]:null; }
			}
			public ICollection<Group> All {
				get { return groups.Values; }
			}
			
			internal void Load(Command.List commands,out int loaded,out int failed,out string error) {
				groups.Clear();
				standard = null;
				loaded = 0; failed = 0; error = null;
				if (!Directory.Exists("groups")) { return; }
				foreach (string file in Directory.GetFiles("groups","*."+host.Extension)) {
					string name = file.Substring(7,file.Length-host.Extension.Length-8);
					Group group = Group.Load(commands,name);
					if (group==null) { failed++; } else {
						if (group.standard) {
							if (standard!=null) { error = "More than one standard group specified."; }
							else { standard = group; }
						} loaded++;
						groups.Add(group.name.ToLower(),group);
					}
				} if (loaded==0) { error = "No groups loaded."; }
				else if (standard==null) { error = "No standard group specified."; }
			}
		}
	}
}
