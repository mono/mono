using System;

public class C
{
	event Func<int, int> E;
	Func<int, int> D;

	public static int Main ()
	{
		var c = new C ();
		Func<int, int> v = Foo;
		dynamic[] arr = new dynamic [] { v };
		
		c.E += arr [0];
		if (c.E.GetInvocationList ().Length != 1)
			return 1;

		c.D += arr [0];
		if (c.D.GetInvocationList ().Length != 1)
			return 2;
		
		return 0;
	}
	
	static int Foo (int ii)
	{
		return 9;
	}
}
