using System;

class Y
{
}

class X
{
	public event Action<int, string> E;

	void Foo ()
	{
		var nc = new Y ();

		E += async (arg1, arg2) => {
			nc = null;
		};

		E (1, "h");
	}

	public static void Main ()
	{
		var x = new X ();
		x.Foo ();
	}
}