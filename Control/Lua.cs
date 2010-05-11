using System;
using System.IO;
using obsidian.Net;
using LuaInterface;

namespace obsidian.Control {
	internal class Lua {
		private Server server;
		private LuaInterface.Lua lua;
		internal string errorLog = null;
		
		internal Lua(Server server) {
			this.server = server;
		}
		
		internal bool Start(string initfile) {
			lua = new LuaInterface.Lua();
			lua["server"] = server;
			lua.RegisterFunction("Is",this,this.GetType().GetMethod("Is"));
			if (!File.Exists(initfile)) {
				server.Log("Initfile '"+initfile+"' not found.");
				return false;
			} try { lua.DoFile(initfile); }
			catch (Exception e) { Error(e); return false; }
			return true;
		}
		
		internal void Stop() {
			lua.Close();
		}
		
		internal void Error(Exception e) {
			if (errorLog!=null) {
				File.AppendAllText(errorLog,e.ToString()+"\n");
				server.Log("Error: "+e.Message);
			} else { server.Log("Error: "+e); }
			if (server.Level!=null) {
				new Message("&eError: "+e.Message).Send(server);
			}
		}
		
		public bool Is(object value,string type) {
			if (value==null) { return false; }
			Type t = value.GetType();
			do if (t.ToString().Equals(type,StringComparison.OrdinalIgnoreCase)) { return true; }
			while ((t = t.BaseType) != null);
			return false;
		}
	}
}
