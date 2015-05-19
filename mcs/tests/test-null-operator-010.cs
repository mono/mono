using System;

class Test
{
	static void Main ()
	{
		Test_1 ("");
		Test_1<object> (null);

		Test_2<object> (null);
		Test_2 ("z");
		Test_2 (0);
		Test_2 ((long?) -8);

		Test_3 (new int[1]);
		Test_3 (new int[] { 5 });
	}

	static void Test_1<T> (T x) where T : class
	{
		x?.Call ();
	}

	static void Test_2<T> (T x)
	{
		x?.Call ();
	}

	static void Test_3<T> (T[] x)
	{
		x[0]?.Call ();
	}
}

static class Ext
{
	public static void Call<T> (this T t)
	{
		Console.WriteLine (typeof (T));
	}
}
