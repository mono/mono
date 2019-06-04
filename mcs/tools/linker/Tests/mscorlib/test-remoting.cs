using System;

public class MyObject : ContextBoundObject
{
}

public class C
{
	public static int Main ()
	{
		var context = new MyObject ();
		return 0;
	}
}
