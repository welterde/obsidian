using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using obsidian.World;

namespace obsidian.Net {
	public class Message {
		private IEnumerable<Packet> packets;
		private static Regex removeInvalid = new Regex(@"[^0-9a-f])");
		private static Regex removeMultiple = new Regex(@"( *?&.)*");
		private static Regex removeEnd = new Regex(@"(&. *)*$");
		private static Regex removeUnused = new Regex(@"((&.)[^&]*)\2");
		
		public Message(string message) {
			message = removeInvalid.Replace(message,"");
			message = removeMultiple.Replace(message,"$+");
			message = removeEnd.Replace(message,"");
			message = removeUnused.Replace(message,"$1");
			if (message[message.Length-1]=='&') {
				message = message.Substring(0,message.Length-1);
			} List<string> lines = new List<string>(message.Split(new char[1]{'\n'},StringSplitOptions.RemoveEmptyEntries));
			// TODO: Continue here ... *yawn*
			/* for (int i=0;i<lines.Count;i++) {
				string line = lines[i];
				if (line.Length<=64) { continue; }
				int lastspace = -1;
				int lastcolor = -1;
				for (int pos=0;pos<line.Length && pos<=64;pos++) {
					if (line[pos]==' ') { lastspace = pos; }
					if (line[pos]=='&') { lastcolor = pos; }
				} 
			} */
			packets = lines.ConvertAll(new Converter<string,Packet>(ToPacket));
		}
		
		private Packet ToPacket(string line) {
			byte id = 0x00;
			if (line.Length>=2) switch (line.Substring(0,2)) {
				case "&e":
					id = 0xFF;
					line = line.Remove(0,2);
					break;
				case "&f":
					line = line.Remove(0,2);
					break;
			} return Protocol.ChatPacket(id,line);
		}
		
		public void Send(Player player) {
			foreach (Packet packet in packets) { packet.Send(player); }
		}
		public void Send(IList<Player> players) {
			foreach (Packet packet in packets) { packet.Send(players); }
		}
		public void Send(IList<Player> players,Player except) {
			foreach (Packet packet in packets) { packet.Send(players,except); }
		}
		public void Send(Level level) {
			foreach (Packet packet in packets) { packet.Send(level); }
		}
		public void Send(Level level,Player except) {
			foreach (Packet packet in packets) { packet.Send(level,except); }
		}
		public void Send(Server server) {
			foreach (Packet packet in packets) { packet.Send(server); }
		}
	}
}
