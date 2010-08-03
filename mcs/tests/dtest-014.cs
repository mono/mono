public class C
{
	public static bool operator == (C a, object b)
	{
		return ReferenceEquals (a, b);
	}

	public static bool operator != (C a, dynamic b)
	{
		return !ReferenceEquals (a, b);
	}
	
	public static decimal operator -(dynamic p1, C p2)
	{
		return 9;
	}
	
	public static int Main ()
	{
		var c = new C ();
		var v = 1 - c;
		
		if (v != 9)
			return 1;
		
		return 0;
	}
}
