using System;

struct S
{
	public static implicit operator string (S s)
	{
		return "1";
	}

	public static implicit operator short? (S s)
	{
		return 1;
	}

	public static implicit operator E (S s)
	{
		return 0;
	}
}

public enum E
{
}

class C
{
	public static int Main ()
	{
		E? e = 0;
		const E e1 = (E)44;
		var res = e == e1;
		if (res != false)
			return 1;

		res = e1 == e;
		if (res != false)
			return 2;

		E e2 = 0;
		S s;
		var res2 = e2 & s;
		if (res2 != 0)
			return 3;

		res2 = s & e2;
		if (res2 != 0)
			return 4;

		return 0;
	}
}