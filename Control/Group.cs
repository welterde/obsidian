using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using obsidian.World;
using obsidian.Data;

namespace obsidian.Control {
	public class Group {
		private static DataHost host = XmlHost.Instance;
		
		#region Members
		private readonly string name;
		private bool canJoin = false;
		private bool canChat = false;
		private bool canBuild = false;
		private bool standard = false;
		private string prefix = "";
		private string postfix = "";
		private List<Command> commands = new List<Command>();
		private Node custom = new Node();
		#endregion
		
		#region Public members
		public string Name {
			get { return name; }
		}
		public bool CanJoin {
			get { return canJoin; }
			set { canJoin = value; }
		}
		public bool CanChat {
			get { return canChat; }
			set { canChat = value; }
		}
		public bool CanBuild {
			get { return canBuild; }
			set { canBuild = value; }
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
		public Node Custom {
			get { return custom; }
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
				string n = (string)node["name"].Value;
				if (n.ToLower()!=name.ToLower()) { return null; }
				Group group = new Group(n);
				group.standard = bool.Parse((string)node["standard"].Value);
				group.prefix = (string)node["prefix"].Value;
				group.postfix = (string)node["postfix"].Value;
				node["privileges"].ListForeach(group.LoadPrivilege);
				group.commands = commands.LoadPermissionList(node["commands"]);
				group.custom = node["custom"]??group.custom;
				return group;
			} catch { return null; }
		}
		public void Save() {
			Node node = new Node();
			node["name"] = new Node(name);
			node["standard"] = new Node(standard.ToString());
			node["prefix"] = new Node(prefix);
			node["postfix"] = new Node(postfix);
			node["privileges"] = SavePrivileges();
			node["commands"] = Command.List.SavePermissionList(commands);
			node["custom"] = custom;
			if (!Directory.Exists("groups")) { Directory.CreateDirectory("groups"); }
			host.Save("groups/"+name+"."+host.Extension,node,"group");
		}
		
		private void LoadPrivilege(Node node) {
			switch ((string)node.Value) {
				case "join": canJoin = true; break;
				case "chat": canChat = true; break;
				case "build": canBuild = true; break;
			}
		}
		private Node SavePrivileges() {
			Node node = new Node();
			node.MakeList();
			if (canJoin) { node.Add(new Node("join")); }
			if (canBuild) { node.Add(new Node("build")); }
			if (canChat) { node.Add(new Node("chat")); }
			return node;
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
