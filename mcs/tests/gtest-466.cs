using System;

class Program
{
	bool Test<T> (T t) where T : class
	{
		return t == this;
	}
	
	public static int Main ()
	{
		var p = new Program ();

		if (p.Test ("a"))
			return 1;
		
		if (!p.Test (p))
			return 2;

		return 0;
	}
}
