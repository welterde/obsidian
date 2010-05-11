using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using obsidian.World;

namespace obsidian.Net {
	public class Message {
		private Packet[] packets;
		private Regex removeMultiple = new Regex(@"( *?&[0-9a-f])*");
		private Regex removeEnd = new Regex(@"(&[0-9a-f] *)*$");
		private Regex removeUnused = new Regex(@"((&[0-9a-f])[^&]*)\2");
		
		public Message(string message) {
			message = removeMultiple.Replace(message,"$+");
			message = removeEnd.Replace(message,"");
			message = removeUnused.Replace(message,"$1");
			// TODO: Split message into multiple lines if too long.
			packets = Array.ConvertAll<string,Packet>(
				message.Split(new char[1]{'\n'},StringSplitOptions.RemoveEmptyEntries),
				new Converter<string,Packet>(ToPacket));
		}
		
		private Packet ToPacket(string line) {
			byte id = 0x00;
			if (line.Length>=2 && line.Substring(0,2)=="&e") {
				id = 0xFF;
				line = line.Remove(0,2);
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
