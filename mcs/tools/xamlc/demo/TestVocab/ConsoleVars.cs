using System.Collections;

namespace Xaml.TestVocab.Console {
	public class ConsoleVars {
		private static Hashtable storage = new Hashtable();
		public static void Set(string name, string o) {
			storage[name] = o;
		}
		public static string Get(string name) {
			return (string)storage[name];
		}
	}
}
