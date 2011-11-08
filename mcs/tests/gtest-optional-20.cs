using System;

public class C
{
	 static void Test<T>(T value, Func<object, T> postProcessor = null)
	{
	}
	
	public static int Main ()
	{
		Test ("");
		return 0;
	}
}

