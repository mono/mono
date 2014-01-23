using System;

public class DieSubrangeType
{
	public int? UpperBound
	{
		get;
		private set;
	}

	public DieSubrangeType ()
	{
		UpperBound = 1;
	}
}

class X
{
	public static int Main ()
	{
		DieSubrangeType subrange = new DieSubrangeType ();
		Console.WriteLine (subrange.UpperBound != null);
		Console.WriteLine ((int) subrange.UpperBound);
		return (int) subrange.UpperBound - 1;
	}
}