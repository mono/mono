// Compiler options: -res:test-464.cs,TEST,private

using System.Reflection;
using System;

class C {		
	public static int Main () 
	{
		string [] s = typeof (C).Assembly.GetManifestResourceNames ();
		if (s [0] != "TEST")
			return 1;
		
		if (typeof (C).Assembly.GetManifestResourceStream ("TEST") == null)
			return 2;

		Console.WriteLine ("OK");
		return 0;
	}
}
