using System;

class C
{
	static void Main ()
	{
		Execute (() => {
			  int a, b;
		  });
	}

	public static void Execute (Action action) { }
}