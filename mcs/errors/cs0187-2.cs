// cs0187.cs: No such operator '++' defined for type 'object'
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
