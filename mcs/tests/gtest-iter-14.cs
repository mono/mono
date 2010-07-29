using System;
using System.Collections.Generic;

class A
{
	protected virtual int BaseM	{
		get {
			return 2;
		}
		set
		{
			throw new ApplicationException ("it should not be called");
		}
	}
}

class B : A
{
	protected override int BaseM {
		set
		{
		}
	}
}

struct S
{
	public IEnumerable<int> GetIt ()
	{
		yield return base.GetHashCode ();
	}
}

class X : B
{
	protected override int BaseM {
		set
		{
			throw new ApplicationException ("it should not be called");
		}
	}

	IEnumerable<int> GetIt ()
	{
		yield return base.BaseM++;
	}

	static int Main ()
	{
		foreach (var v in new X ().GetIt ())
			Console.WriteLine (v);

		foreach (var v in new S ().GetIt ())
			Console.WriteLine (v);

		return 0;
	}
}