using System;
using System.Collections.Generic;

class A
{
	public virtual int Foo ()
	{
		return 4;
	}
}

class C : A
{
	public IEnumerable<int> GetIter ()
	{
		yield return base.Foo ();
	}

	public override int Foo ()
	{
		throw new ApplicationException ();
	}

	public static void Main ()
	{
	}
}