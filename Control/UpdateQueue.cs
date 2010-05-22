using System;
using System.Collections.Generic;
using System.Threading;
using obsidian.Net;
using obsidian.Utility;

namespace obsidian.Control {
	public class UpdateQueue {
		private Server server;
		private LinkedList<Item> items = new LinkedList<Item>();
		private int interval = 80;
		private int max = 3;
		private Thread thread;
		
		public int Interval {
			get { return interval; }
			set {
				if (value<0) { throw new ArgumentOutOfRangeException("value"); }
				interval = value;
			}
		}
		public int MaxUpdatesPerInterval {
			get { return max; }
			set {
				if (value<=0) { throw new ArgumentOutOfRangeException("value"); }
				max = value;
			}
		}
		public int Count {
			get { return items.Count; }
		}
		public bool Running {
			get { return (thread.ThreadState==ThreadState.Running); }
		}
		
		public UpdateQueue(Server server) {
			if (server==null) { throw new ArgumentNullException("server"); }
			this.server = server;
			thread = new Thread(DoStuff);
		}
		
		public void Add(int milliseconds,Action<object> action,object data) {
			DateTime time = DateTime.Now.AddMilliseconds(milliseconds);
			Item i = new Item(time,action,data);
			LinkedListNode<Item> item = items.First;
			if (item!=null) do if (time<item.Value.time) {
				items.AddBefore(item,i);
				return;
			} while ((item = item.Next)!=null);
			items.AddLast(i);
		}
		public void Clear() {
			items.Clear();
		}
		
		public void Start() {
			if (Running) { throw new Exception("UpdateQueue is already running."); }
			thread.Start();
		}
		public void Stop() {
			if (!Running) { throw new Exception("UpdateQueue isn't running."); }
			thread.Abort();
			Clear();
		}
		
		private void DoStuff() {
			while (true) {
				Thread.Sleep(interval);
				int done = 0;
				foreach (Item item in new List<Item>(items)) {
					if (item.time<DateTime.Now.AddMilliseconds(interval/2)) {
						item.action.Raise(server,item.data);
						items.Remove(item);
						done++; if (done>=max) { break; }
					} else { break; }
				}
			}
		}
		
		private class Item {
			public readonly DateTime time;
			public readonly Action<object> action;
			public readonly object data;
			
			public Item(DateTime time,Action<object> action,object data) {
				this.time = time;
				this.action = action;
				this.data = data;
			}
		}
	}
}
