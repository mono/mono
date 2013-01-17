// Compiler options: /r:gtest-098-lib.dll
public class Foo : IFoo
{
	void IFoo.Test<X> ()
	{ }

	void IFoo.Test<Y,Z> ()
	{ }
}

public class Bar<X,Y,Z> : IBar<X>, IBar<Y,Z>
{
	void IBar<X>.Test ()
	{ }

	void IBar<Y,Z>.Test ()
	{ }
}

class X
{
	public static void Main ()
	{ }
}
