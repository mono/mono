//
// This test is for bug #39108. It checks to see that a params method
// is called in its right form.
//
using System;

public class TestParams
{
	public static int Main (string[] args)
	{
		int i;
		
		i = Params (null);
		if (i != 0)
			return 1;

		i = Params ((object) null);
		if (i != 1)
			return 2;

		return 0;
	}
	
	private static int Params (params object[] ps)
	{
		if (ps == null)
			return 0;
		else
			return 1;
	}
}

