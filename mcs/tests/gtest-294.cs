class A {}
class B : A {}

class X
{
	public static A Test (A a, B b)
	{
		return b ?? a;
	}

	public static int Main ()
	{
		A a = new A ();
		B b = new B ();

		if (Test (a, b) != b)
			return 1;

		if (Test (null, b) != b)
			return 2;

		if (Test (a, null) != a)
			return 3;

		if (Test (null, null) != null)
			return 4;

		return 0;

	}
}
