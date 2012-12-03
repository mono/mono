//
// Check that the empty field we produce on empty structs with LayoutKind.Explicit
// has a FieldOffset of zero, or the .NET runtime complains
//
using System;
using System.Reflection;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
struct foo2 {
}

class C
{
	public static int Main ()
	{
		foo2 f = new foo2 ();

		// On .NET if we got this far, we run
		
		Console.WriteLine ("PASS: Test passes on Mono");
		return 0;
    }
}


