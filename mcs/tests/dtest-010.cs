class A
{
	public static T Test<T> (T a, T b)
	{
		return b;
	}
}

class C
{
	int TestCall ()
	{
		return 1;
	}

	public static int Main ()
	{
		dynamic d = 0;
		object o = new C ();

		if (A.Test<dynamic> (d, o).TestCall () != 1)
			return 1;

		if (A.Test (d, o).TestCall () != 1)
			return 2;

		return 0;
	}
}
