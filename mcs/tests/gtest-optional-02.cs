using System;

public class C
{
	public static bool Test3 (int? i = new int ())
	{
		return i == 0;
	}
	
	public static bool Test2 (int? i = null)
	{
		return i == null;
	}

	public static int Test (int? i = 1)
	{
		return i ?? 9;
	}
	
	public static long Test4 (long? i = 5)
	{
		return i.Value;
	}

	public static int Main ()
	{
		if (Test () != 1)
			return 1;
		
		if (Test (null) != 9)
			return 2;
		
		if (!Test2 ())
			return 3;

		if (Test2 (3))
			return 4;

		if (!Test3 ())
			return 5;

		if (Test4 () != 5)
			return 6;
		
		return 0;
	}
}

