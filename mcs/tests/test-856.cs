using System;

public abstract class A : IDisposable
{
	public int i;

	public virtual void Dispose ()
	{
		++i;
	}
}

public abstract class B : A
{
	private new void Dispose ()
	{
		throw new ApplicationException ("B");
	}
}

public class C : B
{
	public static int Main ()
	{
		var c = new C ();
		c.Dispose ();
		if (c.i != 1)
			return 1;

		return 0;
	}

	public override void Dispose ()
	{
		base.Dispose ();
	}
}

