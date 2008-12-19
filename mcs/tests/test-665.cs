using System;

class Test
{
	public static int Main ()
	{
		return checked (Method) (null) + unchecked (Method) (null);
	}
	
	static int Method (object o)
	{
		return 0;
	}
}
