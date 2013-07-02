//
// Test to check that in the 2.x profile, we add the RuntimeCompatibilityAttribute
// if none is specified.  Bug 76364
//
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

class X {
	public static int Main ()
	{
		object [] attrs = typeof (X).Assembly.GetCustomAttributes (true);

		foreach (object o in attrs){
			if (o is RuntimeCompatibilityAttribute){
				RuntimeCompatibilityAttribute a = (RuntimeCompatibilityAttribute) o;

				if (a.WrapNonExceptionThrows)
					return 0;
			}
		}

		// failed, did not find the attribute
		return 1;
	}
}
