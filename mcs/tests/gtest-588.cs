using System;

struct S
{
	public static implicit operator short? (S s)
	{
		return 0;
	}

	public static implicit operator short (S s)
	{
		throw new ApplicationException ();
	}
}

class Program
{
	public static int Main ()
	{
		S? s = null;
		S? s2 = new S ();

		long? i = s;
		if (i != null)
			return 1;

		double? ui = s2;
		if (ui == null)
			return 2;

		return 0;
	}
}