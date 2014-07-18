using System;

class Test
{
	public static int M (bool b = false)
	{
		Console.WriteLine ("PASS");
		return 0;
	}

	public static int M (params string[] args)
	{
		Console.WriteLine ("FAIL");
		return 1;
	}
	
	public static int Main ()
	{
		return M ();
	}
}
