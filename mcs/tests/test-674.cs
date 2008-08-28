using System;

public class Test
{
	delegate int D (int i);
	
	public static int Main ()
	{
		object o = (D) delegate { return 0; };
		((D)o)(1);
		return ((D)o)(1);
	}
}
