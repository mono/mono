namespace Test {
	delegate void Foo (string x, params object [] args);
	class Testee {
		static void Bar (string x, params object [] args) {}
		public static void Main () {
			Foo bar = new Foo (Bar);
			bar ("Hello");
			bar ("Hello", "world");
			bar ("Hello", new string [] { "world" });
			bar ("Hello", "world", "!!!");
			bar ("i = ", 5);
			bar ("x' = ", new object [] {"Foo", 5, 3.6 });
			bar ("x'' = ", "Foo", 5, 3.6);
		}
	}
}
