using System;

class Foo
{
	public virtual void Dyn (out dynamic o)
	{
		o = null;
	}
}

class Bar : Foo
{
	public override void Dyn (out dynamic o)
	{
		base.Dyn (out o);
	}
}

class Program
{
	static void DynOut (out dynamic d)
	{
		d = null;
	}

	static void DynRef (ref object d)
	{
		d = null;
	}

	static int Main ()
	{
		object o;
		DynOut (out o);

		dynamic d = null;
		DynRef (ref d);
		return 0;
	}
}
