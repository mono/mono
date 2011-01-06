// CS0411: The type arguments for method `C.Test<T>(System.Func<T>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 14

using System;

interface IB
{
}

class C
{
	public static void Main ()
	{
		Test (() => { if (true) return (C) null; return (IB) null; });
	}
	
	static void Test<T> (Func<T> f)
	{
	}
}
