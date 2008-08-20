using System;

class Test
{
	// Foo
	static void \u0046oo () 
	{
	}
	
	public static int Main ()
	{
		const string a = "\U00010041";
		const string b = "\U0010FEDC";
		
		Console.WriteLine ((int) a[0]);
		if ((int) a[0] != 55296)
			return 1;

		Console.WriteLine ((int) a[1]);
		if ((int) a[1] != 56320)
			return 2;
		
		Foo ();
		
		return 0;
	}
}

