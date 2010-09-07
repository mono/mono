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

public class C
{
	public void Method_A (ref int i)
	{
	}

	public void Method_B (ref dynamic i)
	{
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
	
	void TestErrorVersions ()
	{
		var c = new C ();
		dynamic d = null;
		c.Method_A (d);
		c.Method_A (d);	
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
