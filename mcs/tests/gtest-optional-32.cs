using System;

abstract class A
{
	public abstract int[] Foo (params int[] args);
}

class B : A
{
	public override int[] Foo (int[] args = null)
	{
		return args;
	}

	static int Main ()
	{
		var b = new B();
		if (b.Foo().Length != 0)
			return 1;

		return 0;
	}
}