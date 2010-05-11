using System;

namespace obsidian.Data {
	internal interface IDataHost {
		string Extension { get; }
		bool Supports(Type type);
		
		Node Load(string filename,out string name);
		void Save(string filename,Node root,string name);
	}
}
