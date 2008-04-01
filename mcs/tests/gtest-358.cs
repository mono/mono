// Tests broken equality and inequality operators

using System;

struct Foo
{
	public static bool operator == (Foo d1, Foo d2)
	{
		throw new ApplicationException ();
	}
		
	public static bool operator != (Foo d1, Foo d2)
	{
		throw new ApplicationException ();	
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
