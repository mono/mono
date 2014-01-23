using System;

class T {
	static Foo GetFoo () { return new Foo (); }

	public static void Main ()
	{
		string s = GetFoo ().i.ToString ();
		Console.WriteLine (s);
	}
}

struct Foo { public int i; }
