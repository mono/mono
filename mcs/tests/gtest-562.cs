interface IFoo { }

abstract class A<T>
{
	public T Value;
}

class B<U> : A<B<U>>, IFoo
{
	public void Test ()
	{
		IFoo foo = this;
		Value = this;
	}
}

class C<U> : A<C<U>.N>, IFoo
{
	public void Test ()
	{
		IFoo foo = this;
		Value = new N ();
	}
	
	public class N
	{
	}
}

class D<U> : A<D<int>>
{
	public void Test ()
	{
		Value = new D<int> ();
	}
}

class E<U> : IFoo where U : A<E<U>>
{
	public void Test (U u)
	{
		IFoo foo = u.Value;
	}
}

static class Application
{
	public static int Main ()
	{
		new B<byte>().Test ();
		new C<char>().Test ();
		new D<string>().Test ();
		
		return 0;
	}
}