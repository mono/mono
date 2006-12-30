// CS0266: Cannot implicitly convert type `int' to `sbyte'. An explicit conversion exists (are you missing a cast?)
// Line: 33

class A3
{
	public static implicit operator sbyte (A3 mask)
	{
		return 1;
	}

	public static implicit operator uint (A3 mask)
	{
		return 6;
	}
	
	public static implicit operator long (A3 mask)
	{
		return 7;
	}

	public static implicit operator ulong (A3 mask)
	{
		return 8;
	}
}


public class C
{
	public static int Main ()
	{
		A3 a3 = null;
		sbyte sa3 = -a3;
	}
}
