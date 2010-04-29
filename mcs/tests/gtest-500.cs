using System;

interface I
{
}

struct S : I
{
}

class Program
{
	static int Main ()
	{
		int? a = 5;
		int? b = 5;
		IComparable ic_a = a;
		IComparable ic_b = b;
		if (ic_a.CompareTo (ic_b) != 0)
			return 1;

		S? s = new S ();
		I iface = s;

		return 0;
	}
}
