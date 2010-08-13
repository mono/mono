using System;

class C<T>
{
	public static int Foo;
}

class X
{
	public static void Main ()
	{
	}
	
	void Test<T> (T A)
	{
		A<T> ();
		
		object C;
		var c = C<int>.Foo;
	}
	
	static void A<U> ()
	{
	}
}
