using foo = Foo;

namespace Foo {
	class A { }
}

class X {
	public static void Main ()
	{
		foo::A a = new Foo.A ();
		System.Console.WriteLine (a.GetType ());
	}
}
