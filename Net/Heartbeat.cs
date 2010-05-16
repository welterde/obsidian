﻿using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;
using System.Web;
using obsidian.World;

namespace obsidian.Net {
	internal class Heartbeat {
		private Server server;
		private Thread thread;
		internal string url;
		internal bool fList = true;
		
		internal bool Running {
			get { return (thread!=null); }
		}
		
		internal Heartbeat(Server server) {
			this.server = server;
		}
		
		internal void Start(int interval) {
			if (Running) { throw new Exception("Heartbeat is already running."); }
			thread = new Thread(delegate() { while (true) { Thread.Sleep(interval); Send(); } });
			thread.Start();
		}
		internal void Stop() {
			if (!Running) { throw new Exception("Heartbeat isn't running."); }
			thread.Abort();
			thread = null;
		}
		
		internal bool Send() {
			string name = "%"+BitConverter.ToString(Encoding.ASCII.GetBytes(server.Name)).Replace("-","%");
			string newUrl = Send("http://www.minecraft.net/heartbeat.jsp",
			           "port="+server.Port+
			           "&max="+server.maxPlayers+
			           "&users="+server.Players.Count+
			           "&name="+name+
			           "&public="+server.Public+
			           "&version="+Protocol.version+
			           "&salt="+server.salt);
			if (newUrl!=null) {
				url = newUrl;
				if (fList) {
					string motd = "%"+BitConverter.ToString(Encoding.ASCII.GetBytes(server.Motd)).Replace("-","%");
					string players = "";
					if (server.Players.Count>0) {
						StringBuilder builder = new StringBuilder();
						foreach (Player player in server.Players) {
							builder.Append(player.Name).Append(",");
						} players = builder.Remove(builder.Length-1,1).ToString();
					} Send("http://list.fragmer.net/announce.php",
					       "name="+name+
					       "&motd="+motd+
					       "&hash="+url.Split('=')[1]+
					       "&max="+server.maxPlayers+
					       "&users="+server.Players.Count+
					       "&public="+server.Public+
					       "&server=obsidian"+
					       "&players="+players);
				} return true;
			} else {
				server.Log("Could not send heartbeat.");
				return false;
			}
		}
		private string Send(string announceUrl,string get) {
			HttpWebRequest request = null;
			HttpWebResponse response = null;
			StreamWriter sw = null;
			try {
				request = (HttpWebRequest)HttpWebRequest.Create(announceUrl);
				request.ContentType = "application/x-www-form-urlencoded";
				request.UserAgent = "Java/1.6.0_13";
				request.Accept = "text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2";
				request.Method = "POST";
				sw = new StreamWriter(request.GetRequestStream());
				sw.Write(get); sw.Close();
				response = (HttpWebResponse)request.GetResponse();
				StreamReader sr = new StreamReader(response.GetResponseStream());
				return sr.ReadToEnd().Split('\r')[0];
			} catch { return null; }
			finally {
				if (request!=null) { request.Abort(); }
				if (response!=null) { response.Close(); }
				if (sw!=null) { sw.Close(); }
			}
		}
	}
}
