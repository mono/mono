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
		Next (arg);
	}
	
	void Next<UU> (UU a) where UU : struct
	{
	}

	public static void Main ()
	{
		new B ().Foo (5);
	}
}
