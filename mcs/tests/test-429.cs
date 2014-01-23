// Compiler options: -warn:0

using global = Foo;

namespace Foo {
	class A { }
}

class A { }

class X {
	public static void Main ()
	{
		A a = new global::A ();
		System.Console.WriteLine (a.GetType ());
	}
}
