using System;

public class NullableAnd
{
	static object Foo (object l, object r)
	{
		return (Boolean?)l & (Boolean?)r;
	}

	public static int Main ()
	{
		var g = Foo (true, true);
		Console.WriteLine (g);
		if ((bool?) g != true)
			return 1;

		return 0;
	}
}