using System;

public class C
{
	static void M<T> (T t) where T : new ()
	{
	}

	public static int Main ()
	{
		M<dynamic> (null);
		return 0;
	}
}
