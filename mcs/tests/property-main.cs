using System;

class X
{
	static int Main ()
	{
		Bar bar = new Bar ();
		if (bar.Message != "bar")
			return 1;

		Foo foo = new Foo ();
		if (foo.Message != "foo")
			return 2;

		Console.WriteLine ("Test ok");
		return 0;
	}
}
