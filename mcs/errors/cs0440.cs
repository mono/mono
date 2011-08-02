// CS0440: An alias named `global' will not be used when resolving `global::'. The global namespace will be used instead
// Line: 5
// Compiler options: -warn:2 -warnaserror

using global = Foo;

namespace Foo {
	class A { }
}

class A { }

class X {
	static void Main ()
	{
		A a = new global::A ();
		System.Console.WriteLine (a.GetType ());
	}
}
