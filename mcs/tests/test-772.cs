using System;

public class Test
{
	static void Main ()
	{
		Foo fu = new Foo (null);
	}
}

class Foo
{
	public Foo (object o)
	{
		throw new ApplicationException ("wrong ctor");
	}

	public Foo (string s, params object[] args)
	{
	}
}
