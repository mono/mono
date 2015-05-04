using System;

public class A
{
	public int Type { get; }

	public A ()
	{
		Type = 2;
	}
}

public class B
{
	static int Type { get; }

	static B ()
	{
		Type = 1;
	}

	static int Main ()
	{
		if (Type != 1)
			return 1;

		var a = new A ();
		if (a.Type != 2)
			return 2;

		return 0;
	}
}