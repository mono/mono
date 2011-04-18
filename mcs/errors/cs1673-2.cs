// CS1673: Anonymous methods inside structs cannot access instance members of `this'. Consider copying `this' to a local variable outside the anonymous method and using the local instead
// Line: 19
using System;

public delegate void Hello ();

struct Foo
{
	public int ID;

	public Foo (int id)
	{
		this.ID = id;
	}

	public void Test (Foo foo)
	{
		Hello hello = delegate {
			Hello (3);
		};
		hello ();
	}

	public void Hello (int value)
	{
		if (ID != value)
			throw new InvalidOperationException ();
	}

	public override string ToString ()
	{
		return String.Format ("Foo ({0})", ID);
	}
}

class X
{
	static void Main ()
	{
		Foo foo = new Foo (3);
		foo.Test (new Foo (8));
	}
}
