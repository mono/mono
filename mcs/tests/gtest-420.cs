using System;
using System.Collections.Generic;

class C
{
}

class TestClass
{
	static int Test (object a, object b, params object[] args)
	{
		return 0;
	}
	
	static int Test (object a, params object[] args)
	{
		return 1;
	}
	
	public static int Main ()
	{
		C c = new C ();
		Test (c, c, new object [0]);
		
		var v = new Func<C, C, object[], int>(Test);
		
		return v (null, null, null);
	}
}
