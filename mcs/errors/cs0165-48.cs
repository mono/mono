// CS0165: Use of unassigned local variable `v'
// Line: 19

using System;

class X
{
	int this [int v] {
		get {
			return 1;
		}
		set {			
		}
	}

	public static void Main ()
	{
		int v;
		X x = null;

		var r = x?[v = 2];
		Console.WriteLine (v);
	}
}