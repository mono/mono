// CS1594: Delegate `Test.Foo' has some invalid arguments
// Line: 10

namespace Test {
	delegate void Foo (string x, params string [] args);
	class Testee {
		static void Bar (string x, params string [] args) {}
		static void Main () {
			Foo bar = new Foo (Bar);
			bar ("x'' = ", "Foo", 5, 3.6);
		}
	}
}
