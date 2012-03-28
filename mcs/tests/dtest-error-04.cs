using System;
using Microsoft.CSharp.RuntimeBinder;

class A
{
	public string Value;
}

public class Test
{
	public static int Main ()
	{
		dynamic d = new A ();
		
		try {
			d.Value = (object)"value";
			return 1;
		} catch (RuntimeBinderException e) {
			if (e.Message != "Cannot implicitly convert type `object' to `string'. An explicit conversion exists (are you missing a cast?)")
				return 2;
		}
		
		return 0;
	}
}
