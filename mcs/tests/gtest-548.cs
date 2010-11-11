using System;

class C
{
	public static int Main ()
	{
		if (null > null)
			return 1;
		
		if ((int?)null > null)
			return 2;

		if (null >= null)
			return 3;
		
		if ((int?)null >= null)
			return 4;

		if (null < null)
			return 10;
		
		if ((int?)null < null)
			return 11;

		if (null <= null)
			return 12;
		
		if ((int?)null <= null)
			return 13;
		
		if ((null * null) != null)
			return 20;

		if ((null / null) != null)
			return 21;

		if ((null % null) != null)
			return 22;

		if ((null - null) != null)
			return 22;

		if ((null >> null) != null)
			return 23;

		if ((null << null) != null)
			return 24;
		
		Console.WriteLine ("ok");
		return 0;
	}
}
