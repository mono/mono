using System;

public class Tmp
{
	public int stuff;
}
public class Driver
{
	Tmp tmp;

	public int? Prop {
		get { return tmp != null ? tmp.stuff : (int?)null; }
	}

	public static int Main ()
	{
		int? r = new Driver().Prop;
		Console.WriteLine (r);
		return r.HasValue ? 1 : 0;
	}
}
