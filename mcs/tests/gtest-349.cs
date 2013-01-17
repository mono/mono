using System.Collections;
using System;
using System.Reflection;

class X
{
	public delegate R Function<T1, T2, R>(T1 arg1, T2 arg2);

	public static int Main ()
	{
		Delegate [] e = new Delegate [] {
			new Function<IList,IList,int> (f2),
			new Function<IList,object,int> (f2)
		};
		
		if ((int)e [0].DynamicInvoke (null, null) != 1)
			return 1;

		if ((int) e [1].DynamicInvoke (null, null) != 2)
			return 2;

		Console.WriteLine ("OK");
		return 0;
	}

	static int f2 (IList self, IList other) { return 1; }
	static int f2 (IList self, object other) {return 2; }
}