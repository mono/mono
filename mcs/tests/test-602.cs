using System;

public class C
{
	public static int Main ()
	{
		int s = Error ("{0} - {0}", "a");
		Console.WriteLine (s);
		
		if (s != 2)
			return 1;
		
		s = Test_A ("aaaa");
		if (s != 1)
			return 2;
		
		s = Test_C (typeof (C), null, null);
		Console.WriteLine (s);
		if (s != 2)
			return 3;
		
		return 0;
	}
		
	static public int Error (string format, params object[] args)
	{
		return Format (format, args);
	}
	
	static int Format (string s, object o)
	{
		return 1;
	}
	
	static int Format (string s, params object[] o)
	{
		return 2;
	}
	
	static int Format (string s, object o, params object[] o2)
	{
		return 3;
	}

	static int Test_A (string s)
	{
		return 1;
	}
	
	static int Test_A (string s, params object[] o)
	{
		return 2;
	}	
	
	static int Test_C (Type t, params int[] a)
	{
		return 1;
	}
	
	static int Test_C (Type t, int[] a, int[] b)
	{
		return 2;
	}	
}