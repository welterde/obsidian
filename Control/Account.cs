using System;
using System.Collections.Generic;
using System.IO;
using obsidian.World;
using obsidian.Data;

namespace obsidian.Control {
	public class Account {
		private static IDataHost host = new XmlHost();
		
		#region Members
		private readonly string name;
		private Group group = null;
		private Player player = null;
		private int logins = 0;
		private TimeSpan total = TimeSpan.Zero;
		private DateTime lastLogin = DateTime.MinValue;
		private DateTime lastLogout = DateTime.MinValue;
		private string lastIP = null;
		private DateTime fileModified = DateTime.MinValue;
		private Node custom = new Node.Compound();
		#endregion
		
		#region Public members
		public string Name {
			get { return name; }
		}
		public Group Group {
			get { return group; }
			set { group = value; }
		}
		public Player Player {
			get { return player; }
		}
		public int Logins {
			get { return logins; }
		}
		public TimeSpan Total {
			get { return total; }
		}
		public DateTime LastLogin {
			get { return lastLogin; }
		}
		public DateTime LastLogout {
			get { return lastLogout; }
		}
		public string LastIP {
			get { return lastIP; }
		}
		public bool Online {
			get { return (player!=null); }
		}
		public Node Custom {
			get { return custom; }
		}
		#endregion
		
		private Account(string name) {
			this.name = name;
		}
		
		private static Account Load(Group.List groups,string name) {
			try {
				string filename = "accounts/"+name+"."+host.Extension;
				string rootName;
				Node node = host.Load(filename,out rootName);
				if (rootName!="account") { return null; }
				string n = (string)node["name"];
				if (n.ToLower()!=name.ToLower()) { return null; }
				Group group = groups[(string)node["group"]];
				if (group==null) { return null; }
				Account account = new Account(n);
				Node statistics = node["statistics"];
				account.group = group;
				account.logins = (int)statistics["logins"];
				account.total = TimeSpan.Parse((string)statistics["total"]);
				account.lastLogin = DateTime.Parse((string)statistics["lastLogin"]);
				account.lastLogout = DateTime.Parse((string)statistics["lastLogout"]);
				account.lastIP = (string)statistics["lastIP"];
				account.fileModified = File.GetLastWriteTime(filename);
				account.custom = (Node.Compound)node["custom"];
				return account;
			} catch { return null; }
		}
		public void Save() {
			Node node = new Node.Compound();
			node["name"] = name;
			node["group"] = group.Name;
			Node statistics = node["statistics"];
			statistics["logins"] = logins;
			statistics["total"] = total.ToString();
			statistics["lastLogin"] = lastLogin.ToString();
			statistics["lastLogout"] = lastLogout.ToString();
			statistics["lastIP"] = lastIP;
			node["custom"] = custom;
			host.Save("accounts/"+name+"."+host.Extension,node,"account");
		}
		
		public class List {
			private Dictionary<string,Account> accounts = new Dictionary<string,Account>();
			private Group.List groups = new Group.List();
			
			public Account this[string name] {
				get { return accounts.ContainsKey(name.ToLower())?accounts[name.ToLower()]:null; }
			}
			
			internal List() {  }
			
			internal void Load(Group.List groups,out int loaded,out int failed) {
				accounts.Clear();
				this.groups = groups;
				loaded = 0; failed = 0;
				if (!Directory.Exists("accounts")) { return; }
				foreach (string file in Directory.GetFiles("accounts","*."+host.Extension)) {
					string name = file.Substring(9,file.Length-host.Extension.Length-10);
					Account account = Account.Load(groups,name);
					if (account==null) { failed++; }
					else { loaded++; accounts.Add(account.name.ToLower(),account); }
				}
			}
			
			internal Account Login(Player player,string name) {
				Account account;
				if (accounts.ContainsKey(name.ToLower())) {
					account = accounts[name.ToLower()];
					if (account.Online) { throw new Exception("Account is already used by another player."); }
					string filename = "accounts/"+name+"."+host.Extension;
					if (File.Exists(filename) && File.GetLastWriteTime(filename)>account.fileModified) {
						Account a = Account.Load(groups,name);
						if (a!=null) { accounts[name.ToLower()] = a; account = a; }
					}
				} else {
					account = new Account(name);
					account.group = groups.Standard;
					accounts.Add(name.ToLower(),account);
				} account.player = player;
				account.lastIP = player.IP;
				account.logins++;
				account.lastLogin = DateTime.Now;
				return account;
			}
			internal void Logout(Account account) {
				if (!account.Online) { throw new Exception("Account isn't used."); }
				account.player = null;
				account.lastLogout = DateTime.Now;
				account.total += account.lastLogout-account.lastLogin;
				account.Save();
			}
		}
	}
}
