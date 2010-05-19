using System;
using System.Runtime.CompilerServices;

interface IFoo
{
	[IndexerName ("Bar")]
	int this[int i] { get; }
}

class Foo : IFoo
{
	public int this[int i] { get { return 42; } }
}

abstract class Bar
{
	[IndexerName ("Baz")]
	public abstract int this[int i] { get; }
}

class Babar : Bar
{
	public override int this[int i] { get { return 42; } }
}

class Test
{
	static int Main ()
	{
		if (typeof (Foo).GetProperty ("Bar") != null)
			return 1;

		if (typeof (Babar).GetProperty ("Baz") == null)
			return 2;

		return 0;
	}
}