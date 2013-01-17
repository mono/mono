using System;

class C<T>
{
	public static void Foo<U> (U arg)
	{
		Action a = () => C<U>.Run ();
		a ();
	}
	
	static void Run ()
	{
	}
}

class A
{
	public static void Main ()
	{
		C<int>.Foo<long> (8);
	}
}