
// Tests variable type inference with the var keyword when using the for-statement

using System;

public class Test
{
	public static int Main ()
	{
		for (var i = 0; i < 1; ++i)
			if (i.GetType() != typeof (int))
				return 1;
		
		return 0;
	}
}

