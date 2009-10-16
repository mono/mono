using System;

public class TS
{
	public int? v { 
		get { return (int?) this; }
	}

	public static implicit operator int? (TS s)
	{
		return 5;
	}

	public static implicit operator TS (int? date)
	{
		return null;
	}

	public static int Main ()
	{
		var r = new TS ().v;
		if (r != 5)
			return 1;

		return 0;
	}
}
