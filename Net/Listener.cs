using System;
using System.Net;
using System.Net.Sockets;

namespace obsidian.Net {
	internal class Listener {
		private TcpListener listen;
		internal bool Running { get { return (listen!=null); } }
		internal event Action<Socket> AcceptEvent = delegate {  };
		
		internal bool Start(ushort port) {
			if (Running) { throw new Exception("Listener is already running."); }
			listen = new TcpListener(IPAddress.Any,port);
			try { listen.Start(); }
			catch { Stop(); return false; }
			listen.BeginAcceptSocket(new AsyncCallback(Accept),null);
			return true;
		}
		internal void Stop() {
			if (!Running) { throw new Exception("Listener isn't running."); }
			listen.Stop();
			listen = null;
		}
		private void Accept(IAsyncResult result) {
			Socket socket = listen.EndAcceptSocket(result);
			listen.BeginAcceptSocket(new AsyncCallback(Accept),null);
			AcceptEvent(socket);
		}
	}
}