using System;

class B : A
{
	public static void Foo (int i)
	{
	}
}

class A
{
	public static void Foo (string s)
	{
	}
}


public static class Test
{
	public static void Main ()
	{
		B.Foo ("a");
	}
}
