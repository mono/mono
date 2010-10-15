using System;
using Microsoft.CSharp.RuntimeBinder;

public class Test
{
	void Foo ()
	{
	}
	
	public static int Main ()
	{
		dynamic d = new Test ();

		var t1 = true ? d : 2;
		t1.Foo ();
		
		var t2 = true ? 1 : d;
		if (t2 != 1)
			return 1;
		
		try {
			t2.Foo ();
			return 2;
		} catch (RuntimeBinderException) {
		}

		return 0;
	}
}