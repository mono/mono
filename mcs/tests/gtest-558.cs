using System;

abstract class A<T>
{
	public abstract void Foo<U> (U arg) where U : T;
}

class B : A<int>
{
	public override void Foo<U> (U arg)
	{
		ValueType vt = arg;
	}

	public static void Main ()
	{
		new B ().Foo (5);
	}
}
