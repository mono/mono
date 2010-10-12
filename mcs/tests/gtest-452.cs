using System;

public class Test
{
	public static int Main ()
	{
		S mc = new S ();
		float? f = mc;
		if (f != 5)
			return 1;
		
		return 0;
	}
}

struct S
{
	public static implicit operator float (S p1)
	{
		throw new ApplicationException ("should not be called");
	}
	
	public static implicit operator float? (S p1)
	{
		return 5;
	}
}
