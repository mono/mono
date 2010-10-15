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
	
	public static int M (string a, string b)
	{
		return 5;
	}
	
	public static int M (ref object o, out dynamic d)
	{
		d = null;
		return 1;
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
		c.Method_B (d);	
	}

	static int Main ()
	{
		object o;
		DynOut (out o);

		dynamic d = null;
		DynRef (ref d);
		
		dynamic d1 = 1, d2;
		
		// This should not involve runtime binder
		if (C.M (ref d1, out d2) != 1)
			return 1;
		
		return 0;
	}
}
