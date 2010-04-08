using System;

interface I<T>
{
}

class A : I<int>
{
}

class B : A
{
}

class M
{
	static void Test<T> (I<T> f)
	{
	}
	
	public static void Main ()
	{
		Test (new A ());
		Test (new B ());
	}
}
