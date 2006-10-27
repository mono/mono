// CS0131: The left-hand side of an assignment must be a variable, a property or an indexer
// Line: 12

using System;
using System.Collections;

class Test {
	public static void Main(string[] args) {
		ArrayList al = new ArrayList();
		al[0] = 0;
		
		Console.WriteLine(((int)al[0])++);
	}
}
