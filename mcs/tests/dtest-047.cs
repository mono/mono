using System;

class C<T>
{
	public event Func<int, int> E = l => l + 9;
	
	public static int Test (int arg)
	{
		dynamic c = new C<T> ();
		return c.E (arg);
	}
}

public class Test
{
	public static int Main ()
	{
		if (C<int>.Test (5) != 14)
			return 1;
		
		return 0;
	}
}
