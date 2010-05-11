using System;
using System.IO;
using System.IO.Compression;

namespace obsidian.Utility {
	internal static class GZipper {
		internal static byte[] GZip(params byte[][] arrays) {
			MemoryStream ms = new MemoryStream();
			GZipStream gs = new GZipStream(ms,CompressionMode.Compress,true);
			foreach (byte[] array in arrays) {
				gs.Write(array,0,array.Length);
			} gs.Close();
			ms.Position = 0;
			byte[] bytes = new byte[ms.Length];
			ms.Read(bytes,0,(int)ms.Length);
			ms.Close();
			return bytes;
		}
	}
}
