using System;
using System.Linq.Expressions;

struct S<T> where T : struct
{
	public static int Test ()
	{
		Expression<Func<T?, bool>> e = (T? o) => o == null;
		if (!e.Compile ().Invoke (null))
			return 1;
		
		if (e.Compile ().Invoke (default (T)))
			return 2;
		
		Console.WriteLine ("OK");
		return 0;
	}
}

class C
{
	public static int Main()
	{
		return S<int>.Test ();
	}
}

