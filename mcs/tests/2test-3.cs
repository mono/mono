using System;
using System.Collections;

class X {
	static IEnumerable GetIt ()
	{
		Console.WriteLine ("hello");
		yield 1;
		yield 2;
		yield 3;
	}
	
	static void Main ()
	{
		foreach (int i in GetIt ()){
			Console.WriteLine ("Value=" + i);
		}
		
	}
}
