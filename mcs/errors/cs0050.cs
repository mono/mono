// CS0050: Inconsistent accessibility: return type `X' is less accessible than method `Foo.Bar()'
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

