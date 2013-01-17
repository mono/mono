using System;
using System.Reflection;

class X {
	delegate object test (MethodInfo x);
		
	public static void Main ()
	{
		DoCall (delegate(MethodInfo from) {
                    return from.Invoke (null, new object[] { from });
                });
	}

	static void DoCall (test t)
	{
	}
}
