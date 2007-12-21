// CS1502: The best overloaded method match for `C.Test_C(System.Type, params int[])' has some invalid arguments
// Line: 10

using System;

public class C
{
	public static int Main ()
	{
		return Test_C (typeof (C), null, null);
	}
	
	static int Test_C (Type t, params int[] a)
	{
		return 1;
	}
}
