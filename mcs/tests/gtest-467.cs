using System;

struct S
{
	public static int Main ()
	{
		S? s = null;
		A a = s;
		B b = (B) s;
		return 0;
	}
}

class A
{
	public static implicit operator A (S x)
	{
		return new B ();
	}
}

class B : A
{
}