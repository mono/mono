// cs0050.cs: Inconsistent accessibility. Return type less accessible than method.
// Line: 7

using System;

class Foo {
	public static Foo Bar () {
		return new Foo ();
	}

	public static void Main () {
		Foo x = Bar ();
	}
}

