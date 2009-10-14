// CS0023: The `++' operator cannot be applied to operand of type `object'
// Line: 9

using System;
using System.Collections;

class Test {
	public static void Main(string[] args) {
		ArrayList al = new ArrayList();
		al[0] = 0;
		
		Console.WriteLine((al[0])++);
	}
}
