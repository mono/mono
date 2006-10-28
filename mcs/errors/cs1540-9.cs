// CS1540: Cannot access protected member `A.Test(int)' via a qualifier of type `B'. The qualifier must be of type `C' or derived from it
// Line: 28
using System;

public abstract class A
{
	protected virtual void Test (int a)
	{ }

	public void Test ()
	{ }
}

public class B : A
{
	protected override void Test (int a)
	{
		base.Test (a);
	}
}

public class C : A
{
	private B B;

	protected override void Test (int a)
	{
		B.Test (a);
		base.Test (a);
	}
}

class X
{
	static void Main ()
	{ }
}
