// cs0050.cs: Inconsistent accessibility. Return type less accessible than method.
// Line: 13

using System;

class X {
	public X ()
	{
	}
}

public class Foo {
	public static X Bar () {
		return new Foo ();
	}

	public static void Main () {
		Foo x = Bar ();
	}
}

