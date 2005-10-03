// cs0266-5.cs: Cannot implicitly convert type `long' to `int'. An explicit conversion exists (are you missing a cast?)
// Line: 9

class X
{
	public static void Main ()
	{
		int i = 3;
		i += 999999999999999;
	}
}

