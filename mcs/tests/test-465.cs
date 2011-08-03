// Compiler options: -res:test-465.cs,

using System.Reflection;
using System;

class C {		
	public static int Main () 
	{
		string [] s = typeof (C).Assembly.GetManifestResourceNames ();
		if (s [0] != "test-465.cs")
			return 1;
		
		if (typeof (C).Assembly.GetManifestResourceStream ("test-465.cs") == null)
			return 2;

		Console.WriteLine ("OK");
		return 0;
	}
}

