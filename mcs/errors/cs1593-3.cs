// CS1593: Delegate `System.Func<int,int>' does not take `2' arguments
// Line: 11


using System;

class C
{
	static void Main (string [] args)
	{
		M ((x, y) => 2);
	}

	static void M (Func<int, int> a)
	{
	}
}
