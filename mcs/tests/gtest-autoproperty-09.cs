using System;

struct S
{
	public static int P { get; } = 4;

	public static int[] PA { get; } = { 0, 2 };

	public static int Main ()
	{
		if (P != 4)
			return 1;

		if (PA [1] != 2)
			return 10;

		var c = new C ();
		if (c.P != -3)
			return 2;

		if (c.P2 != 1)
			return 3;

		c.P2 = 9;
		if (c.P2 != 9)
			return 4;

		var s = new S2 (null);
		if (s.P != 4)
			return 12;

		if (s.P2 != 1)
			return 13;

		s.P2 = 9;
		if (s.P2 != 9)
			return 14;

		return 0;
	}
}

class C
{
	public decimal P { get; } = -3;
	public int P2 { get; set; } = 1;
}

struct S2
{
	public int P { get; } = 4;
	public int P2 { get; set; } = 1;

	public S2 (object o)
	{
	}
}