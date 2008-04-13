using System;

class C
{
	public static int Main ()
	{
		int? i = 1;
		try {
			i = checked(int.MaxValue + i);
			return 1;
		} catch (OverflowException)	{
			return 0;
		}
	}
}
