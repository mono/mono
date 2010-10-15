using System;

class C
{
	public static int Main ()
	{
		var a = new { ToString = 1 };
		if (a.ToString != 1)
			return 1;
		
		return 0;
	}
}