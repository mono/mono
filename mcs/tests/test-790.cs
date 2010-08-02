struct S
{
	public S (double d)
	{
	}
}

enum E
{
}

struct Test
{
	static void Verify_1 (out Test a, out Test b)
	{
		a = b = new Test ();
	}

	static void Verify_2 (ref S a, ref S b)
	{
		a = b = new S (4.31);
	}

	static void Verify_3 (out E a, out E b)
	{
		a = b = new E ();
	}

	public static int Main ()
	{
		Test t1, t2;
		Verify_1 (out t1, out t2);

		S s1, s2;
		Verify_2 (ref s1, ref s2);

		E e1, e2;
		Verify_3 (out e1, out e2);
		return 0;
	}
}

