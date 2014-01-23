using System;

public class X {
	public static bool Compute (int x)
	{
		return x == null;
	}

	public static bool Compute2 (int x)
	{
		return x != null;
	}
	
	public static int Main ()
	{
		if (Compute (1) != false)
			return 1;

		if (Compute2 (1) != true)
			return 1;

		return 0;
	}
}
