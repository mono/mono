class C
{
	public int M (object d, long s)
	{
		return 1;
	}
	
	public int M (long s, object d)
	{
		return 2;
	}

	public int M (dynamic d, dynamic s)
	{
		return 3;
	}
	
	public int M2 (object d)
	{
		return 1;
	}
	
	public int M2 (byte s)
	{
		return 2;
	}
}

public class Test
{
	public static int Main ()
	{
		dynamic d = new C ();
		byte s = 5;
		object o = 2;
		int v = d.M (s, o);
		
		if (v != 2)
			return 1;
		
		v = d.M2 (1 + 3);
		if (v != 2)
			return 2;

		return 0;
	}
}
