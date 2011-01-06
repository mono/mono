using System;

class A
{
	public static int Main ()
	{
		var list = new A ();
		var a = list as object;
		object o = a;
		return 0;
	}
}