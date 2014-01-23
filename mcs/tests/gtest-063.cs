using System;

public class Test
{
	public static int IndexOf (Array array, object value)
	{
		// This is picking the non-generic version.
		return IndexOf (array, value, 0, array.Length);
	}

	public static int IndexOf (Array array, object value, int startIndex, int count)
	{
		return 2;
	}

	public static int IndexOf<T> (T[] array, T value, int startIndex, int count)
	{
		return 1;
	}
}

class X
{
	public static int Main ()
	{
		Test test = new Test ();
		string[] array = new string [] { "Hello" };

		int result = Test.IndexOf (array, array);
		if (result != 2)
			return 1;

		string hello = "Hello World";
		// This is picking the generic version.
		result = Test.IndexOf (array, hello, 1, 2);
		if (result != 1)
			return 2;

		return 0;
	}
}
