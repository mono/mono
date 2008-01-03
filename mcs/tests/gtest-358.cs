// Tests broken equality and inequality operators

using System;

struct Foo
{
	public static bool operator == (Foo d1, Foo d2)
	{
		return false;
	}
		
	public static bool operator != (Foo d1, Foo d2)
	{
		return true;
	}
}

public class Test
{
	static Foo ctx;

	public static void Main ()
	{
		if (ctx == null)
			return;
		
		if (ctx != null)
			return;
	}
}
