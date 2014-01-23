using System;

class X
{
	protected virtual int Foo ()
	{
		return 1;
	}

	protected delegate int FooDelegate ();
	protected FooDelegate foo;

	protected X ()
	{
		foo = new FooDelegate (Foo);
	}
}

class Y : X
{
	protected Y ()
		: base ()
	{ }

	protected override int Foo ()
	{
		return 2;
	}

	int Hello ()
	{
		return foo ();
	}

	public static void Main ()
	{
		Y y = new Y ();
		int result = y.Hello ();

		if (result == 2)
			Console.WriteLine ("OK");
		else
			Console.WriteLine ("NOT OK");
	}
}
