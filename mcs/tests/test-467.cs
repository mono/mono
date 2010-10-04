using System;

class A
{
	public delegate int D ();
}

class B : A
{
	new delegate void D ();
}

class C
{
	static int Foo ()
	{
		return 1;
	}
	
	public static int Main ()
	{
		A.D d = new B.D (Foo);
		if (d () != 1)
			return 1;
		
		return 0;
	}
}