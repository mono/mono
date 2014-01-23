public interface IFoo
{
	void Test<T> ();

	void Test<U,V> ();
}

public class Foo : IFoo
{
	void IFoo.Test<X> ()
	{ }

	void IFoo.Test<Y,Z> ()
	{ }
}

public interface IBar<T>
{
	void Test ();
}

public interface IBar<U,V>
{
	void Test ();
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
