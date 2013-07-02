using foo = Foo;

namespace Foo {
	class A { }
}

class X {
	public static void Main ()
	{
		Foo.A a = new foo::A ();
		System.Console.WriteLine (a.GetType ());
	}
}
