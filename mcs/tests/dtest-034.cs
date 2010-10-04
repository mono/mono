public class C
{
	public void M<U, V> (out U u, ref V v)
	{
		u = default (U);
	}
}

public class Test
{
	public static int Main ()
	{
		dynamic u = "s";
		dynamic v = 5;
		dynamic c = new C ();
		c.M (out u, ref v);
		
		if (v != 5)
			return 1;
			
		if (u != null)
			return 2;

		return 0;
	}
}


