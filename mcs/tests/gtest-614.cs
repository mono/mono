using System;

struct S
{
	public static explicit operator int? (S? s)
	{
		throw new ApplicationException ();
	}

	public static implicit operator int (S? s)
	{
		return 2;
	}
}

class C
{
	public static int Main()
	{
		int? nn = 3;
		S? s = new S ();
		int? ret = s ?? nn;
		if (ret != 2)
			return 1;

		return 0;
	}
}
