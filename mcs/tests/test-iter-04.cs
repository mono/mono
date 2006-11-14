// Compiler options: -langversion:default

using System;
using System.Collections;

class X {
	static IEnumerable GetRange (int start, int end)
	{
		for (int i = start; i < end; i++)
			yield return i;
	}

	static void Main ()
	{
		Console.WriteLine ("GetRange 10..20");
				   
		foreach (int i in GetRange (10, 20)){
			Console.WriteLine ("i=" + i);
		}
	}
}
