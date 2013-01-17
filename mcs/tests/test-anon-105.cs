using System;
using System.Collections.Generic;

public delegate void Hello ();

struct Foo
{
	public int ID;

	public Foo (int id)
	{
		this.ID = id;
	}

	public IEnumerable<Foo> Test (Foo foo)
	{
		yield return this;
		yield return foo;
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
		foreach (Foo bar in foo.Test (new Foo (8)))
			Console.WriteLine (bar);
	}
}
