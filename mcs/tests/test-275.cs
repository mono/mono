using System;
using System.Reflection;
using System.Runtime.CompilerServices;

public delegate void DelType ();

struct S
{
	public event DelType MyEvent;
}

public class Test
{
	public event DelType MyEvent;

	public static int Main ()
	{
		EventInfo ei = typeof (Test).GetEvent ("MyEvent");
		MethodImplAttributes methodImplAttributes = ei.GetAddMethod ().GetMethodImplementationFlags ();

		if ((methodImplAttributes & MethodImplAttributes.Synchronized) == 0) {
			Console.WriteLine ("FAILED");
			return 1;
		}

		methodImplAttributes = ei.GetRemoveMethod ().GetMethodImplementationFlags ();
		if ((methodImplAttributes & MethodImplAttributes.Synchronized) == 0) {
			Console.WriteLine ("FAILED");
			return 2;
		}

		ei = typeof (S).GetEvent ("MyEvent");
		methodImplAttributes = ei.GetAddMethod ().GetMethodImplementationFlags ();

		if ((methodImplAttributes & MethodImplAttributes.Synchronized) != 0) {
			Console.WriteLine ("FAILED");
			return 3;
		}

		methodImplAttributes = ei.GetRemoveMethod ().GetMethodImplementationFlags ();
		if ((methodImplAttributes & MethodImplAttributes.Synchronized) != 0) {
			Console.WriteLine ("FAILED");
			return 4;
		}

		return 0;
	}
}
