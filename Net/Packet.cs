using System;
using System.Collections.Generic;
using obsidian.World;

namespace obsidian.Net {
	public class Packet {
		private readonly byte[] buffer;
		
		public Packet(byte[] buffer) {
			this.buffer = buffer;
		}
		
		#region Sending
		public void Send(Player player) {
			player.Send(buffer);
		}
		public void Send(IEnumerable<Player> players) {
			foreach (Player player in players) Send(player);
		}
		public void Send(IEnumerable<Player> players,Player except) {
			foreach (Player player in players)
				if (player!=except) { Send(player); }
		}
		public void Send(Level level) {
			Send(new List<Player>(level.Players));
		}
		public void Send(Level level,Player except) {
			Send(new List<Player>(level.Players),except);
		}
		public void Send(Server server) {
			Send(server.Level);
		}
		#endregion
	}
}
