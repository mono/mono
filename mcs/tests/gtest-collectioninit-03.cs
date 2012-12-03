using System.Collections;

class X
{
	public static int Main ()
	{
		object x = null;
		R (ref x);
		return ((Hashtable)x).Count == 1 ? 0 : 1;
	}

	static void R (ref object o)
	{
		o = new Hashtable () { { 1, 2 } };
	}
}
