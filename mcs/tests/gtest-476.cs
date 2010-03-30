using System;

// Obsolete attribute cased early inflation of B without IFoo being defined for it

interface IFoo
{
}

class Test<T> : IFoo
{
	public B First ()
	{
		return new B ();
	}
	
	public IFoo Second ()
	{
		return new B ();
	}

	[Obsolete]
	public struct B : IFoo
	{
	}
}

class C
{
	public static void Main ()
	{
	}
}
