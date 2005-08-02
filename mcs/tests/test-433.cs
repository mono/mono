using foo = Foo;

namespace Foo {
	class A { }
}

class X {
	static foo::A a = new Foo.A ();
	static void Main ()
	{
		System.Console.WriteLine (a.GetType ());
	}
}
