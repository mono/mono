using System;

public class C
{
	public D D { get; private set; }
	public string Value { get; private set; }
	public Foo Foo { get; set; }

	public int Test ()
	{
		dynamic d = new C ();
		return D.Foo (d.Value);
	}

	public static int Test2 (dynamic d)
	{
		return Foo.Method(d);
	}

	public static int Main ()
	{
		var c = new C ();
		if (c.Test () != 1)
			return 1;

		if (C.Test2 ("s") != 1)
			return 2;
		
		return 0;
	}
}

public struct D
{
	public int Foo (string value)
	{
		return 1;
	}
}

public class Foo
{
	public static int Method (string s)
	{
		return 1;
	}
}
