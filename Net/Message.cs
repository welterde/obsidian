﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using obsidian.World;

namespace obsidian.Net {
	public class Message {
		private IEnumerable<Packet> packets;
		private static Regex removeInvalid = new Regex(@"&[^0-9a-f]");
		private static Regex removeMultiple = new Regex(@"( *?&.)*");
		private static Regex removeEnd = new Regex(@"(&. *)*$");
		private static Regex removeUnused = new Regex(@"((&.)[^&]*)\2");
		private static int wordwrap = 12;
		
		public Message(string message) {
			message = removeInvalid.Replace(message,"");
			List<string> lines = new List<string>(message.Split(new char[1]{'\n'},StringSplitOptions.RemoveEmptyEntries));
			for (int i=0;i<lines.Count;i++) {
				string line = lines[i];
				line = removeMultiple.Replace(line,"$+");
				line = removeEnd.Replace(line,"");
				line = removeUnused.Replace(line,"$1");
				if (line[line.Length-1]=='&') {
					line = line.Substring(0,line.Length-1);
				} if (line.Length<=64) { continue; }
				int lastspace = line.LastIndexOf(' ',63,wordwrap);
				if (lastspace!=-1) { line = line.Remove(lastspace,1); }
				int pos = (lastspace!=-1)?lastspace:64;
				int lastcolor = line.LastIndexOf('&',pos-1,pos);
				string color = "";
				if (lastcolor!=-1) {
					color = line.Substring(lastcolor,2);
				} if (lastcolor>=pos-2 && lastcolor<pos) {
					line = line.Remove(lastcolor,2);
					pos = lastcolor;
				}lines[i] = line.Substring(0,pos);
				lines.Insert(i+1,color+"-> "+line.Substring(pos));
			} packets = lines.ConvertAll(new Converter<string,Packet>(ToPacket));
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
		
		#region Sending
		public void Send(Player player) {
			foreach (Packet packet in packets) packet.Send(player);
		}
		public void Send(IEnumerable<Player> players) {
			foreach (Packet packet in packets) packet.Send(players);
		}
		public void Send(IEnumerable<Player> players,Player except) {
			foreach (Packet packet in packets) packet.Send(players,except);
		}
		public void Send(Level level) {
			foreach (Packet packet in packets) packet.Send(level);
		}
		public void Send(Level level,Player except) {
			foreach (Packet packet in packets) packet.Send(level,except);
		}
		public void Send(Server server) {
			foreach (Packet packet in packets) packet.Send(server);
		}
		#endregion
	}
}
