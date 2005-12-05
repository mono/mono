using System;

public delegate void MyDelegate (int a);

public class X
{
	static event MyDelegate e = X.Test;
	static int cc = 4;
	
	static void Test (int foo)
	{
		X.cc = foo;
		Console.WriteLine ("OK");
	}
	
	public static int Main ()
	{
		e (10);
		if (cc != 10)
			return 1;
		
		return 0;
	}
}
