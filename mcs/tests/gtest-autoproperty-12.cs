using System;

public class A
{
	public int X { get; }
	public virtual int Y { get; }

	public A ()
	{
		X = 4;
		X++;

		Y = 2;
		Y++;
	}
}

class B : A
{
	int i_get;

	public override int Y { get { ++i_get; return base.Y; } }

	public static int Main ()
	{
		var a = new A ();
		if (a.X != 5)
			return 1;

		if (a.Y != 3)
			return 2;

		var b = new B ();
		if (b.X != 5)
			return 3;

		if (b.i_get != 1)
			return 4;

		if (b.Y != 3)
			return 5;

		if (b.i_get != 2)
			return 6;

		return 0;
	}
}
