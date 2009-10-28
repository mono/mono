using System;
using System.Linq.Expressions;

class M
{
	public static void Foo<T> (Expression<Func<T, T>> x)
	{
	}
	
	static string Param (string b)
	{
		Expression<Func<string, Expression<Func<string>>>> e = (string s) => () => b;
		return e.Compile () ("-##54!2").Compile () ();
	}

	public static int Main ()
	{
		Foo<int> ((i) => i);

		Foo ((int i) => i);

		Expression<Func<int, int>> func = (i) => i;
		Foo (func);
		
		if (Param ("my test") != "my test")
			return 1;
		
		return 0;
	}
}

