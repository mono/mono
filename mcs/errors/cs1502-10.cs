// CS1502: The best overloaded method match for `foo.p(string, object, object)' has some invalid arguments
// Line: 24

using System;

public class foo
{
	static int intval = 3;

	public static void voidfunc()
	{
	}
	
	static void p (string s, object o1, object o2)
	{
	}
	
	static void p (string s, params object[] o)
	{
	}

	public static void Main()
	{
		p ("Whoops: {0} {1}", intval, voidfunc());
	}
}
