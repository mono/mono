class X
{
	static int field;

	public static int Main ()
	{
		Test () = 3;

		if (field != (byte) 3)
			return 1;

		G<string>.Test (ref field) = 6;
		if (field != 6)
			return 2;

		--Test ();
		if (field != 5)
			return 3;

		Test (ref Test (), ref Test ());

		return 0;
	}

	static ref int Test ()
	{
		return ref field;
	}

	static void Test<T> (ref T a, ref int b)
	{
	}

	static void Test2<T> (ref T arg)
	{
		Test (ref arg, ref Test ());
	}
}

class G<U>
{
	public static ref T Test<T> (ref T arg)
	{
		return ref arg;
	}
}