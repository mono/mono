using foo = Foo;

namespace Foo {
	class A { }
}

class X {
	static void Main ()
	{
		foo::A a = new Foo.A ();
		System.Console.WriteLine (a.GetType ());
	}
}
