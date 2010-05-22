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
			string shortMsg = "Error: "+e.Message.Split(new char[1]{'\n'},2)[0];
			string longMsg = "Error: "+e;
			if (e is LuaScriptException) {
				LuaScriptException ex = (LuaScriptException)e;
				if (ex.IsNetException) { e = e.InnerException; }
				shortMsg = "Error in "+ex.Source+e.Message.Split(new char[1]{'\n'},2)[0];
				longMsg = "Error in "+ex.Source+e;
			} if (errorLog!=null) {
				File.AppendAllText(errorLog,longMsg+"\n");
				server.Log(shortMsg);
			} else { server.Log(longMsg); }
			if (server.Level!=null) {
				new Message("&e"+shortMsg).Send(server);
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
