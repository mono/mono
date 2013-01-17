using System;

class Program
{
	public static int Test(Func<object> func)
	{
		func ();
		return 1;
	}

	public static int Test(Func<string> func)
	{
		func ();
		return 2;
	}
	
	public static int Main()
	{
		if (Test (() => (dynamic) 1) != 1)
			return 1;
		
		return 0;
	}
}
