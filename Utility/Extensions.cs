using System;
using obsidian.Net;

namespace obsidian.Utility {
	public static class Extensions {
		public static void Raise(this Delegate del,Server server,params object[] args) {
			if (server!=null) {
				lock (server.lua) {
					try { del.DynamicInvoke(args); }
					catch (Exception e) { server.lua.Error(e.InnerException); }
				}
			} else { del.DynamicInvoke(args); }
		}
	}
}
