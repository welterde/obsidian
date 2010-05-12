using System;
using System.Text.RegularExpressions;

namespace obsidian.Utility {
	public static class RegexHelper {
		private static Regex alnum = new Regex("[a-zA-Z0-9]+");
		private static readonly Regex name = new Regex(@"^[a-zA-Z\d\._]{1,16}$");
		private static readonly Regex chat = new Regex(@"^[ -%'-~]*$");
		
		public static bool IsAlphaNumeric(string value) {
			return alnum.IsMatch(value);
		}
		public static bool IsValidName(string value) {
			return name.IsMatch(value);
		}
		public static bool IsValidChat(string value) {
			return chat.IsMatch(value);
		}
	}
}
