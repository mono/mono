// cs0131.cs: The left-hand side of an assignment or mutating operation must be a variable, property or indexer
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
