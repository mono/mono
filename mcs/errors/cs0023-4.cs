using System;
class Test {
	public static void Main(string[] argv) {
		Console.WriteLine("Type of anonymous block: {0}",
			(delegate() {}).GetType());
	}
}
