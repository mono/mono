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
	static int Main ()
	{
		foo2 f = new foo2 ();

		// On .NET if we got this far, we run
		// On Mono, we are going to actually use an internal routine to check if the offset is there
		//
		Type fit = typeof (FieldInfo);
		MethodInfo gfo = fit.GetMethod ("GetFieldOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		if (gfo == null){
			Console.WriteLine ("PASS: On MS runtime, Test OK");
			return 0;
		}
		
		Type t = typeof (foo2);
		FieldInfo fi = t.GetField ("$PRIVATE$", BindingFlags.Instance | BindingFlags.NonPublic);

		object res = gfo.Invoke (fi, null);
		if (res.GetType () != typeof (Int32))
			return 1;
		int r = (int) res;
		if (r != 0){
			Console.WriteLine ("FAIL: Offset is not zero");
			return 1;
		}
		Console.WriteLine ("PASS: Test passes on Mono");
		return 0;
    }
}


