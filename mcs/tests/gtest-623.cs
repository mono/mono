// Compiler options: -r:gtest-623-lib.dll

using System;

public class C
{
	static bool Test<T> () where T : struct, I
	{
		var t = new T ();
		if (t.GetValue () != 3)
			return false;

		return true;
	}

	public static int Main ()
	{
		if (!Test<S> ())
			return 1;

		Console.WriteLine ("ok");
		return 0;
	}
}