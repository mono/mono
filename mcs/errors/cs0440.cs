// cs0440.cs: An alias named `global' will not be used when resolving 'global::'; the global namespace will be used instead
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
