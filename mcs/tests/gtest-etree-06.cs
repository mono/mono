using System;
using System.Linq.Expressions;

public class Test
{
	public static int Main()
	{
		Expression<Func<int>> f = (() => Value());
		return f.Compile ().Invoke ();
	}

	private static int Value()
	{
		return 0;
	}
}
