using System;
using Microsoft.CSharp.RuntimeBinder;

class A
{
	private class N
	{
		public void Foo ()
		{
		}
	}
	
	public static dynamic Factory ()
	{
		return new N ();
	}
}

public class Test
{
	public static int Main ()
	{
		dynamic d = A.Factory ();
		
		try {
			d.Foo ();
			return 1;
		} catch (RuntimeBinderException e) {
			if (e.Message != "`A.N.Foo()' is inaccessible due to its protection level")
				return 2;
		}
		
		return 0;
	}
}
