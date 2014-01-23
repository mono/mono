using System;

struct Foo
{
	public int Value;

	public Foo (int value)
	{
		this.Value = value;
	}

	public static Foo operator - (Foo? f)
	{
		if (f.HasValue)
			return new Foo (-f.Value.Value);

		return new Foo (42);
	}
}

struct Bar
{
	public int Value;

	public Bar (int value)
	{
		this.Value = value;
	}

	public static Bar? operator - (Bar? b)
	{
		if (b.HasValue)
			return new Bar (-b.Value.Value);

		return b;
	}
}

class Test
{

	static Foo NegateFoo (Foo f)
	{
		return -f;
	}

	static Foo NegateFooNullable (Foo? f)
	{
		return -f;
	}

	static Bar? NegateBarNullable (Bar? b)
	{
		return -b;
	}

	static Bar? NegateBar (Bar b)
	{
		return -b;
	}

	public static int Main ()
	{
		if (NegateFooNullable (null).Value != 42)
			return 1;

		if (NegateFoo (new Foo (2)).Value != -2)
			return 2;

		if (NegateBarNullable (null) != null)
			return 3;

		if (NegateBar (new Bar (2)).Value.Value != -2)
			return 4;

		Console.WriteLine ("OK");
		return 0;
	}
}
