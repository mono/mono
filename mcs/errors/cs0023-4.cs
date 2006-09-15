// CS0023: The `.' operator cannot be applied to operand of type `anonymous method'
// Line: 8

using System;
class Test {
	public static void Main(string[] argv) {
		Console.WriteLine("Type of anonymous block: {0}",
			(delegate() {}).GetType());
	}
}
