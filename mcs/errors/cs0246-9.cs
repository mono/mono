// CS0246: The type or namespace name `DllImport' could not be found. Are you missing an assembly reference?
// Line: 16

using System;
using System.Threading;

public class Test
{
	static void Main ()
	{
		var tr = new Thread (delegate () {
			Foo ();
		});
	}

	[DllImport ("Foo")]
	extern static void Foo ();
} 
