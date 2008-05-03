// CS0029: Cannot implicitly convert type `int' to `System.EventHandler'
// Line: 12

using System;

class C
{
	static event EventHandler h;
	
	public static void Main ()
	{
		h = 0;
	}
}
