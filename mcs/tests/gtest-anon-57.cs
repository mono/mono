using System;

delegate void Foo ();

interface IFoo<T>
{
	void Test ();
}

class X<T> : IFoo<T>
{
	void IFoo<T>.Test ()
	{
		Foo foo = delegate {
			Console.WriteLine (1);
		};
		
		foo ();
	}
}

class M
{
	static void Main ()
	{
		IFoo<int> x = new X<int> ();
		x.Test ();
	}
}
