using System;

struct S
{
	public static bool operator == (S s, S i)
	{
		throw new ApplicationException ();
	}

	public static bool operator != (S s, S i)
	{
		throw new ApplicationException ();
	}
}

struct S2
{
	public static int counter;

	public static bool operator == (S2 s, S2 i)
	{
		counter++;
		return true;
	}

	public static bool operator != (S2 s, S2 i)
	{
		throw new ApplicationException ();
	}
}


struct S3
{
	public static int counter;

	public static implicit operator int?(S3 arg)
	{
		counter++;
		return null;
	}
}

class C
{
	public static int Main ()
	{
		S? s = new S ();
		S? s2 = null;
		S? s4 = null;

		if ((s == s2) != false)
			return 1;

		if ((s2 == s) != false)
			return 2;

		if ((s2 == s4) != true)
			return 3;

		S x = new S ();

		if ((s2 == x) != false)
			return 5;

		if ((x == s2) != false)
			return 6;

		S2? s2_1 = new S2 ();
		S2? s2_3 = new S2 ();
		S2 x2 = new S2 ();

		if ((s2_1 == s2_3) != true)
			return 7;

		if ((s2_1 == x2) != true)
			return 8;

		if ((x2 == s2_1) != true)
			return 9;

		if (S2.counter != 3)
			return 10;

		S3 s3;

		if ((s3 == null) != true)
			return 20;

		if ((null == s3) != true)
			return 21;

		if (S3.counter != 2)
			return 22;

		return 0;
	}
}