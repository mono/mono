public class Foo {
	delegate Inner foo_fn(string s);
	public static void Main(string[] args)
	{
		foo_fn f = delegate (string s) {
			return new Inner(s + s);
		};
		f (args[0]);
	}

	class Inner
	{
		public Inner (string s)
		{ }
	}
}
