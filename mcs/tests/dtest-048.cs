using System;
class A
{
	static int M (string s, object o)
	{
		return 1;
	}

	static int M (string s, params object[] o)
	{
		if (o != null)
			return 2;

		return 0;
	}

	public static int Main ()
	{
		if (M ("x", null) != 0)
			return 1;
		
		if (M ("x", (object[])null) != 0)
			return 2;
		
		if (M ("x", (dynamic)null) != 0)
			return 3;
		
		return 0;
	}
}