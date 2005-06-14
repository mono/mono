public class Foo {
	delegate Inner foo_fn(string s);
	public static void Main()
	{
		foo_fn f = delegate (string s) {
			return new Inner(s + s);
		};
		f ("Test");
	}

	class Inner
	{
		public Inner (string s)
		{ }
	}
}
