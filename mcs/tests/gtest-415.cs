using System;

class Foo
{
	public static int Main ()
	{
		if (Bar (1))
			return 1;

		if (!Bar (IntPtr.Zero))
			return 2;

		return 0;
	}

	static bool Bar<T> (T val) where T : struct
	{
		return val is IntPtr;
	}
}
