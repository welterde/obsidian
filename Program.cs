using System;
using System.Threading;
using obsidian.Net;
using obsidian.World;
using obsidian.Data;
using obsidian.Control;

namespace obsidian {
	class Program {
		private static void Main(string[] args) {
			Blocktype.Init();
			ThreadPool.SetMaxThreads(20,100);
			new Server(Console.Out).Start(args);
			Thread.Sleep(Timeout.Infinite);
		}
	}
}