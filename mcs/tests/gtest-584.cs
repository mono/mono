using System;

enum E : sbyte
{
	V = 1
}

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

	public static implicit operator int? (S s)
	{
		throw new ApplicationException ();
	}

	public static implicit operator E? (S s)
	{
		return null;
	}
}

class C
{
	public static int Main ()
	{
		E? a = E.V;
		E? a_n = null;
		E? b = E.V;
		E? b_n = null;

		if (a != b)
			return 1;

		if (a == a_n)
			return 2;

		if (a_n != b_n)
			return 3;
		
		E e = (E) 4;
		S s;
		if (e == s)
			return 10;

		if (s == e)
			return 11;

		if (e > s)
			return 12;

		if (s > e)
			return 13;

		if ((s & e) != null)
			return 14;

		if ((s & e) != null)
			return 15;

		Console.WriteLine ("ok");

		return 0;
	}
}