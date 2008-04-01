struct S
{
	public static bool operator == (S a, S b)
	{
		return true;
	}
	
	public static bool operator != (S a, S b)
	{
		return true;
	}
}

public class C
{
	static int Print (S? i)
	{
		if (i != null)
			return 5;
		
		return 0;
	}
	
	public static int Main ()
	{
		return Print (null);
	}
}