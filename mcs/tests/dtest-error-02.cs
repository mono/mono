using System;
using Microsoft.CSharp.RuntimeBinder;

class A
{
	private class N
	{
		public void Foo ()
		{
		}

		public int Property { get; set; }
		
		string this [int index] {
			get {
				return "x";
			}
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
			if (e.Message != "'object' does not contain a definition for 'Foo'")
				return 2;
		}
		
		try {
			var x = d.Property;
			return 3;
		} catch (RuntimeBinderException e) {
			if (e.Message != "'object' does not contain a definition for 'Property'")
				return 4;
		}

		try {
			var x = d [4];
			return 5;
		} catch (RuntimeBinderException e) {
			if (e.Message != "Cannot apply indexing with [] to an expression of type 'object'")
				return 6;
		}

		return 0;
	}
}
