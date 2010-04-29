using System;
using System.Reflection;

public class C
{
	public static int Test<T> (T[] t)
	{
		// Has to include readonly. prefix
		return t[0].GetHashCode ();
	}

	public static int TestExtra<T> (T[,] t)
	{
		// Has to include readonly. prefix
		return t[0, 0].GetHashCode ();
	}

	public static int Main ()
	{
		Test (new[] { 2.1, 4.5 });
		Test (new[] { "b" });

		var body = typeof (C).GetMethod ("Test").GetMethodBody ();

		// Check for readonly. (0xFE1E)
		var array = body.GetILAsByteArray ();
		if (array[2] != 0xFE)
			return 1;

		if (array[3] != 0x1E)
			return 1;

		return 0;
	}
}
