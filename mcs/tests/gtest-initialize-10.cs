using System;

class Foo
{
	public int P { get; set; }
}

class Y
{
	public static int Main ()
	{
		Foo foo = new Foo ();
		foo.P = 1;

		if (!Do (foo))
			return 1;

		Console.WriteLine ("OK");
		return 0;
	}

	static bool Do (Foo f)
	{
		f = new Foo () {
			P = f.P
		};

		if (f.P != 1)
			return false;

		Foo f2 = new Foo ();
		f2.P = 9;
		f2 = new Foo () {
			P = f2.P
		};

		if (f2.P != 9)
			return false;

		return true;
	}
}