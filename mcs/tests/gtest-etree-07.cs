using System;
using System.Linq.Expressions;

delegate void EmptyDelegate ();

class C
{
	static int i;
	
	static void Test ()
	{
		i += 9;
	}
	
	public static int Main ()
	{
		Expression<Func<EmptyDelegate>> e = () => new EmptyDelegate (Test);
		
		if (e.Body.ToString () != "Convert(CreateDelegate(EmptyDelegate, null, Void Test()))")
			return 1;

		var v = e.Compile ();
		v.Invoke ()();
		
		if (i != 9)
			return 2;
		
		Expression<Func<EmptyDelegate>> e2 = () => Test;
		if (e2.Body.ToString () != "Convert(CreateDelegate(EmptyDelegate, null, Void Test()))")
			return 1;

		var v2 = e2.Compile ();
		v2.Invoke ()();
		
		if (i != 18)
			return 2;

		return 0;
	}
}
