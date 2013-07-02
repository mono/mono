// Compiler options: -o+

using System;
using System.Reflection;

struct D
{
	public static D d1 = new D (1);
	public int d2;

	public D (int v)
	{
		d2 = 0;
	}
}

class T
{
	public static int Main ()
	{
		ConstructorInfo mi = typeof(D).GetConstructors (BindingFlags.Instance | BindingFlags.Public)[0];
        MethodBody mb = mi.GetMethodBody();
		
		Console.WriteLine (mb.GetILAsByteArray ().Length);
		if (mb.GetILAsByteArray ().Length != 8) {
			return 1;
		}

		mi = typeof (D).GetConstructors (BindingFlags.Static | BindingFlags.NonPublic) [0];
		mb = mi.GetMethodBody ();

		Console.WriteLine (mb.GetILAsByteArray ().Length);
		if (mb.GetILAsByteArray ().Length != 12) {
			return 2;
		}
			
		Console.WriteLine ("OK");
		return 0;
	}
}
