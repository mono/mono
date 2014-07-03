// CS0457: Ambiguous user defined operators `D.implicit operator D(System.Action)' and `D.explicit operator D(Foo)' when converting from `method group' to `D'
// Line: 25

using System;

public delegate void Foo ();

class D
{
	public static implicit operator D (Action d)
	{
		return new D ();
	}

	public static explicit operator D (Foo d)
	{
		return new D ();
	}
}

class Program
{
	static void Main()
	{
		D d = (D) Main;
	}
}
