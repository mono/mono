using System;

class A<D1, D2, D3>
{
	public virtual void Foo<T> () where T : D2
	{
	}
}

class B<DD2> : A<int, DD2, short>
{
}

class C : B<string>
{
	public override void Foo<T> ()
	{
	}
}

public class Program
{
	static void Main ()
	{
		new C ().Foo<string> ();
	}
}
