using System;

public class TestC
{
	public static int Main ()
	{
		if (Test<string> () () != typeof (string)) return 1;
		if (Test<int> () () != typeof (int)) return 2;
		Console.WriteLine ("ok");
		return 0;
	}

	public static Func<Type> Test<T> ()
	{
		return () => typeof (T);
	}
}
