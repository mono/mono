using System;

class A
{
	public static int operator + (short x, A b)
	{
		return -1;
	}
	
	public static int operator - (A a)
	{
		return -1;
	}
}

class B : A
{
	public static int operator + (int x, B d)
	{
		return 1;
	}
	
	public static int operator - (B b)
	{
		return 1;
	}
}

class C : B
{
}

public class Test
{
	public static int Main ()
	{
		var b = new B ();
		short s = 3;
		var res = s + b;

		if (res != 1)
			return 1;
		
		var c = new C ();
		if (-c != 1)
			return 2;

		return 0;
	}
}