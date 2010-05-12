using System;
using System.Collections.Generic;
using System.Threading;

namespace obsidian.World.NPCs {
	public class Mimic : Body {
		private readonly Body body;
		private bool active = false;
		private Queue<Position> queue = new Queue<Position>();
		
		public Body Body {
			get { return body; }
		}
		public bool Active {
			get { return active; }
			set {
				if (value==active) { return; }
				if (value) { Activate(); }
				else { Deactivate(); }
			}
		}
		
		public Mimic(Body body) {
			if (body==null) { throw new ArgumentNullException("body"); }
			this.body = body;
			name = body.Name;
			level = body.Level;
			Visible = body.Visible;
			Activate();
		}
		
		private void Activate() {
			Position.Set(body.Position);
			body.MoveEvent += OnMove;
			body.Destroyed += OnDestroy;
		}
		private void Deactivate() {
			body.MoveEvent -= OnMove;
			body.Destroyed -= OnDestroy;
		}
		
		private void OnMove(Body body) {
			queue.Enqueue((Position)body.Position.Clone());
			new Thread(
				delegate() {
					Thread.Sleep(300);
					Position.Set(queue.Dequeue());
				}).Start();
		}
		private void OnCreate(Body body) {
			Visible = true;
		}
		private void OnDestroy(Body body) {
			Visible = false;
		}
	}
}
