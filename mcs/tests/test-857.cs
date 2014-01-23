using System;

public class Outer
{
	public enum Inner
	{
		ONE,
		TWO
	}
}

public class TypeHiding
{

	public static bool Test1 (Outer Outer)
	{
		return 0 == Outer.Inner.ONE;
	}

	public static bool Test2 ()
	{
		Outer Outer = null;
		return 0 == Outer.Inner.ONE;
	}

	public static void Main ()
	{
	}
}