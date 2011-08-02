// CS0266: Cannot implicitly convert type `Bar' to `X'. An explicit conversion exists (are you missing a cast?)
// Line: 18

public enum Bar
{
	ABar
}

class X
{
	public static explicit operator X (Bar the_bar)
	{
		return new X();
	}
	
	public static void Main ()
	{
		X x = Bar.ABar;
	}
}
