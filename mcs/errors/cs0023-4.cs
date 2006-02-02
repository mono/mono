// cs0023-4.cs: The `.' operator can not be applied to anonymous methods
// Line: 8

using System;
class Test {
	public static void Main(string[] argv) {
		Console.WriteLine("Type of anonymous block: {0}",
			(delegate() {}).GetType());
	}
}
