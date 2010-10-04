using System;

class A
{
	public int value;
	
	public A (int value)
	{
		this.value = value;
	}
}

class B
{
	static void Foo (int i, out A a)
	{
		a = new A (i);
	}

	public static int Main ()
	{
		dynamic d = 6;
		A a;
		Foo (d, out a);
		if (a.value != 6)
			return 1;
		
		return 0;
	}
}

