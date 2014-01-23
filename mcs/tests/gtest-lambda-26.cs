using System;

class C
{
	public static void Main ()
	{
		Execute (() => {
			  int a, b;
		  });
	}

	public static void Execute (Action action) { }
}