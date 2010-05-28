using System;

public abstract class A
{
	public virtual int Test ()
	{
		throw new ApplicationException ();
	}
}

public class B : A
{
	public override int Test ()
	{
		Console.WriteLine ("B");
		return 1;
	}
	
	public void Test (object[] builders)
	{
	}

	public virtual void Test (object[] builders, string s)
	{
	}
}

public class C : B
{
	public override void Test (object[] builders, string s)
	{
	}
}

public class D : C
{
	public override int Test ()
	{
		return base.Test ();
	}
}

class T
{
	public static int Main ()
	{
		var v = new D ();
		if (v.Test () != 1)
			return 1;
		
		return 0;
	}
}