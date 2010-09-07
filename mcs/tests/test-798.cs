using System;

class A
{
	public void Foo (out int a)
	{
		a = 100;
	}
}

class B : A
{
	public void Foo (ref int a)
	{
		throw new ApplicationException ("should not be called");
	}
}

class C
{
	public static int Main ()
	{
		int x;
		new B().Foo (out x);
		if (x != 100)
			return 1;
		
		return 0;
	}
}