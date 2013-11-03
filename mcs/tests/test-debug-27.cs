using System;

// Tests for explicit call sequence point

struct S
{
	public S (int i)
	{

	}

	public static implicit operator int (S s)
	{
		return 1;
	}
}

class C
{
	public static int A ()
	{
		return 1;
	}

	public static int B (C c)
	{
		return 2;
	}

	public static C Test ()
	{
		return new C ();
	}

	public string Foo ()
	{
		return null;
	}

	void Test_1 ()
	{
		Func<int> f = A;

		var res = f () + f ();
	}

	void Test_2 ()
	{
		var s = new S (0);
	}

	void Test_3 ()
	{
		int i = new S () + new S ();
	}

	void Test_4 ()
	{
		Test ().Foo ();
	}

	static int Main ()
	{
		return 0;
	}
}


