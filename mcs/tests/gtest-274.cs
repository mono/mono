using System;

public struct Foo
{
	public readonly long Value;

	public Foo (long value)
	{
		this.Value = value;
	}

	public static implicit operator Foo (long value)
	{
		return new Foo (value);
	}
}

public struct Bar
{
	public readonly Foo Foo;

	public Bar (Foo foo)
	{
		this.Foo = foo;
	}

	public static implicit operator Bar (Foo foo)
	{
		return new Bar (foo);
	}
}

public struct Baz
{
	public readonly Foo Foo;

	public Baz (Foo foo)
	{
		this.Foo = foo;
	}

	public static explicit operator Baz (Foo foo)
	{
		return new Baz (foo);
	}
}

class X
{
	public static void Main ()
	{
		int a = 3;
		int? b = a;
		int? b0 = null;

		Foo? f1 = a;
		Foo? f2 = b;
		Foo? f3 = b0;
		Foo f4 = (Foo) b;

		Bar? b1 = f1;
		Bar? b2 = f2;
		Bar? b3 = f3;
		Bar b4 = (Bar) f2;

		Baz? z1 = (Baz?) f1;
		Baz? z2 = (Baz?) f2;
		Baz? z3 = (Baz?) f3;
		Baz z4 = (Baz) f2;
	}
}
