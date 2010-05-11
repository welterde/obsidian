using System;

namespace obsidian.World.NPCs {
	public class Mimic {
		private readonly Player player;
		private readonly Body body;
		
		public Player Player {
			get { return player; }
		}
		
		public Mimic(Player player) {
			this.player = player;
			body = new Body(player.Name,this,player.Level);
			body.Position.Set(player.Position);
			player.MoveEvent += delegate { body.Position.Set(player.Position); };
			player.DisconnectedEvent += OnDisconnect;
			body.Create();
		}
		
		public void Destroy() {
			if (!body.Visible) { throw new Exception("Mimic is already destroyed."); }
			player.DisconnectedEvent -= OnDisconnect;
			body.Destroy();
		}
		
		private void OnDisconnect(Player player,string message) {
			Destroy();
		}
	}
}
