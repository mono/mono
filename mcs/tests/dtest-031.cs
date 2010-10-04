using System;

public class A<T>
{
	public U CustomDelegate<U>(out U u)
	{
		u = default(U);
		return default(U);
	}
}

public class Test
{
	public static int Main()
	{
		Foo<int> ();
		return 0;
	}
	
	static void Foo<Z> ()
	{
		dynamic a = new A<Z>();
		Z z;
		a.CustomDelegate(out z);
	}
}