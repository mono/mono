// cs0121.cs: ambigous call when selecting function due to implicit casts
// Line: 15

class X {
	static void a (int i, double d)
	{
	}

	static void a (double d, int i)
	{
	}

	public static void Main ()
	{
		a (0, 0);
	}
}	
