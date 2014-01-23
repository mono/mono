using System;

class Foo { }

static partial class Extensions
{
	public static bool IsFoo (this Foo self)
	{
		return true;
	}
}

class Bar { }

partial class Extensions
{
	public static bool IsBar (this Bar self)
	{
		return true;
	}
}

class Program
{
	public static void Main ()
	{
	}
}
