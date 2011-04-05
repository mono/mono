using System;

struct Program
{
	static int Test<T> (T o)
	{
		return o is Program? ? 1 : 2;
	}
	
	public static int Main ()
	{
		if (Test (5) != 2)
			return 1;
		
		Program? a = new Program ();
		if (Test (a) != 1)
			return 2;
		
		return 0;
	}
}