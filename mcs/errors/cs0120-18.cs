// CS0120: An object reference is required to access non-static member `Derived.Foo()'
// Line: 16

using System;

public class Base
{
	public Base (Action a)
	{
	}
}

public class Derived : Base
{
	public Derived ()
		: base (() => Foo ())
	{
	}

	void Foo ()
	{
	}
}