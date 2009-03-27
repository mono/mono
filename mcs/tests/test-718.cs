using System;

class A
{
	public static void Foo (int x, int y)
	{
	}
}

sealed class B : A
{
	public static void Main ()
	{
		Foo (1, 2);
	}
	
	void Foo (int i)
	{
	}
}
