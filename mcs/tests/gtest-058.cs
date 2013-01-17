class Foo {
	public static void Main () {}
}

class Foo <T> {
	static Foo <T> x;
	static Foo <T> Blah { get { return x; } }
}
