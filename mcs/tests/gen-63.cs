using System;

public class Test
{
	public int Length {
		get {
			return 1;
		}
	}

	public static implicit operator object[] (Test test)
	{
		return new object[1] { test };
	}

	public static int IndexOf (Test array, object value)
	{
		// The non-generic function is better than the generic one whose type
		// arguments have been infered.
		return IndexOf (array, value, 0, array.Length);
	}

	public static int IndexOf (Test array, object value, int startIndex, int count)
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
	static int Main ()
	{
		Test test = new Test ();
		int result = Test.IndexOf (test, test);
		if (result == 2)
			return 0;
		else
			return 1;
	}
}
