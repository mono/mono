using System;

static class C
{
	public static int Test (this int a, int b = 5, string s = "")
	{
		return a * 3 + b;
	}
	
	static T Foo<T> (T t, int a)
	{
		return t;
	}
	
	static void Lambda (Func<int, int> a)
	{
		a (6);
	}
	
	public static int Main ()
	{
		if (2.Test () != 11)
			return 1;
			
		if (1.Test (b : 2) != 5)
			return 2;
		
		if (Foo ("n", a : 4) != "n")
			return 3;
		
		if (Foo (t : "x", a : 4) != "x")
			return 4;
		
		Lambda (a : (a) => 1);
		
		// Hoisted variable
		int var = 8;
		Lambda (a : (a) => var);
		
		Console.WriteLine ("ok");
		return 0;
	}
}
