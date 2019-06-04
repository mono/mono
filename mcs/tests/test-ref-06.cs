using System;

class X
{
	public static int Main ()
	{
		var x = new X ();
		x [0] = 3;
		if (x.field != 3)
			return 1;
		x.Prop = 5;
		if (x.field != 5)
			return 2;

		return 0;
	}

	int field;

	ref int this [int idx] => ref field;

	ref int Prop => ref field;

}