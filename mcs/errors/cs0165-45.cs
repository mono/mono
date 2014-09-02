// CS0165: Use of unassigned local variable `v'
// Line: 19

using System;

class X
{
	void Foo (out int value)
	{
		value = 1;
	}

	public static void Main ()
	{
		int v;
		X x = null;

		x?.Foo (out v);
		Console.WriteLine (v);
	}
}