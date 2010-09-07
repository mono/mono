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

		return 0;
	}
}
