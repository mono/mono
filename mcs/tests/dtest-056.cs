using System;

public class C
{
	public D D { get; private set; }
	public string Value { get; private set; }

	public int Test ()
	{
		dynamic d = new C ();
		return D.Foo (d.Value);
	}

	public static int Main ()
	{
		var c = new C ();
		if (c.Test () != 1)
			return 1;
		
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
