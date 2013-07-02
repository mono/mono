using System;

class Foo {
	public bool IsBar {
		get { return false; }
	}
}

static class FooExt {

	public static bool IsBar (this Foo f)
	{
		return f.IsBar;
	}
}

class Repro {

	public static void Main ()
	{
		var f = new Foo ();
		Console.WriteLine (f.IsBar ());
	}
}
