using System;

class Test
{    
	public static int Main()
	{
		if (!("aoeu" is String))
			return 1;
			
		if (!("aoeu" is Object))
			return 2;
			
		return 0;
	}
}
