using System;
using System.Collections.Generic;
using obsidian.World;
using obsidian.Net;
using obsidian.Data;

namespace obsidian.Control {
	public class Command {
		#region Members
		private readonly string name;
		private readonly string syntax;
		private readonly string help;
		internal readonly UseHandler use;
		public delegate void UseHandler(Command command,Player player,string message);
		#endregion
		
		#region Public members
		public string Name {
			get { return name; }
		}
		public string Syntax {
			get { return syntax; }
		}
		public string Help {
			get { return help; }
		}
		#endregion
		
		private Command(string name,string syntax,string help,UseHandler use) {
			this.name = name;
			this.syntax = "/"+name;
			if (syntax!="") { this.syntax += " "+syntax; }
			this.help = this.syntax+" - "+help;
			this.use = use;
		}
		
		public void Use(Player player,string message) {
			use(this,player,message);
		}
		
		public class List {
			private Server server;
			private Dictionary<string,Command> commands = new Dictionary<string,Command>();
			
			public Command this[string name] {
				get { return commands.ContainsKey(name.ToLower())?commands[name.ToLower()]:null; }
			}
			
			internal List(Server server) {
				this.server = server;
			}
			
			public Command Create(string name,string syntax,string help,UseHandler use) {
				int index = name.LastIndexOf(' ');
				if (index!=-1) {
					string cmd = name.Substring(0,index);
					Command supCommand = this[cmd];
					if (supCommand==null) { throw new Exception("Command '"+cmd+"' doesn't exist."); }
				} Command command = new Command(name,syntax,help,use);
				Command old = this[name];
				if (old!=null) {
					commands.Remove(name.ToLower());
					foreach (Group group in server.Groups.All)
						if (group.Commands.Contains(old)) {
						group.Commands.Remove(old);
						group.Commands.Add(command);
					}
				} commands.Add(name.ToLower(),command);
				return command;
			}
			
			public Command Search(ref string message) {
				string[] split = message.Split(' ');
				string command = split[0];
				if (split.Length==1) { message = ""; }
				else { message = message.Remove(0,split[0].Length+1); }
				for (int i=1;i<split.Length;i++) {
					if (commands.ContainsKey((command+" "+split[i]).ToLower())) {
						command += " "+split[i];
						if (i==split.Length-1) { message = ""; }
						else { message = message.Remove(0,split[i].Length+1); }
					} else { break; }
				} return this[command];
			}
			
			internal void Init() {
				commands.Clear();
				Create("info","","Displays information on the server.",
				       delegate (Command command,Player player,string message) {
				       	if (message!="") {
				       		new Message("&eSyntax: "+command.syntax).Send(player);
				       		return;
				       	} new Message("&eCustom Minecraft server 'obsidian'.").Send(player);
				       });
				Create("help","[<command>|commands]","Displays generic help or information on a specific command.",
				       delegate (Command command,Player player,string message) {
				       	if (message=="") {
				       		new Message(server.Help).Send(player);
				       		return;
				       	} command = this[message];
				       	if (command==null) { new Message("&eThere is no help available for '"+message+"'.").Send(player); }
				       	else { new Message("&e"+command.help).Send(player); }
				       });
				Create("help commands","","Shows all available commands.",
				       delegate (Command command,Player player,string message) {
				       	if (message!="") {
				       		new Message("&eSyntax: "+command.syntax).Send(player);
				       		return;
				       	} string cmds = "";
				       	for (int i=0;i<player.Group.Commands.Count;i+=1) {
				       		string cmd = player.Group.Commands[i].name;
				       		if (cmd.IndexOf(' ')!=-1) { continue; }
				       		cmds += ", "+cmd;
				       	} cmds = cmds.Remove(0,2);
				       	new Message("&eAvailable commands: "+cmds).Send(player);
				       });
				Create("bind","<block> [<replace>]","Replaces blocks built by a specific blocktype.",
				       delegate (Command command,Player player,string message) {
				       	if (message=="") {
				       		new Message("&eSyntax: "+command.syntax).Send(player);
				       		return;
				       	} string[] args = message.Split(new char[1]{' '},2);
				       	Blocktype block = null;
				       	byte id;
				       	if (byte.TryParse(args[0],out id)) {
				       		block = Blocktype.FindById(id);
				       	} else { block = Blocktype.FindByName(args[0]); }
				       	if (block==null) {
				       		new Message("&eUnknown blocktype '"+args[0]+"'.").Send(player);
				       		return;
				       	} if (!block.Placeable) {
				       		new Message("&e"+block.Name+" is not placeable.").Send(player);
				       		return;
				       	} if (args.Length==1) {
				       		if (player.bind[block.ID]==block.ID) {
				       			new Message("&e"+block.Name+" isn't bound.").Send(player);
				       			return;
				       		} player.bind[block.ID] = block.ID;
				       		new Message("&eUnbound "+block.Name+".").Send(player);
				       	} else {
				       		Blocktype replace = null;
				       		if (byte.TryParse(args[1],out id)) {
				       			replace = Blocktype.FindById(id);
				       		} else { replace = Blocktype.FindByName(args[1]); }
				       		if (replace==null) {
				       			new Message("&eUnknown blocktype '"+args[1]+"'.").Send(player);
				       			return;
				       		} Node node = player.Group.Custom["bind"];
				       		if (node==null || !node.Contains(replace.Name.ToLower())) {
				       			new Message("&eYou can't bind something to "+replace.Name+".").Send(player);
				       			return;
				       		} if (player.bind[block.ID]==replace.ID) {
				       			new Message("&e"+block.Name+" is already bound to "+replace.Name+".").Send(player);
				       			return;
				       		} player.bind[block.ID] = replace.ID;
				       		new Message("&eBound "+block.Name+" to "+replace.Name+".").Send(player);
				       	}
				       });
			}
			
			internal List<Command> LoadPermissionList(Node node) {
				List<Command> cmds = new List<Command>();
				foreach (Node n in (List<Node>)node.Value) {
					Command command = this[(string)n.Value];
					if (command!=null) { cmds.Add(command); }
				} return cmds;
			}
			internal static Node SavePermissionList(List<Command> cmds) {
				Node node = new Node();
				foreach (Command command in cmds) {
					node.Add(new Node(command.name));
				} return node;
			}
		}
	}
}
