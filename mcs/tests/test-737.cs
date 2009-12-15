using System;

public struct Breaks
{
	private double val;

	public double this[int i, int j]
	{
		get { return val; }
		set { val = value; }
	}

	public Breaks (double val)
	{
		this.val = val;
	}
}

public class Tester
{
	public static int Main ()
	{
		Breaks b = new Breaks (3.0);
		b[0, 0] += 3.0;
		if (b[0, 0] != 6.0)
			return 1;

		return 0;
	}
}
