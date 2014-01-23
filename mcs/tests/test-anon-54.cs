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
		Foo bar = this;
		Hello hello = delegate {
			foo.Hello (8);
			bar.Hello (3);
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
	public static void Main ()
	{
		Foo foo = new Foo (3);
		foo.Test (new Foo (8));
	}
}
