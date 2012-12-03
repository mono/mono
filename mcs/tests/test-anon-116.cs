// Bug #79972
delegate void TestFunc<T>(T val);

class A
{
	public A(TestFunc<int> func) { }

	public static void Main ()
	{ }
}

class TestClass
{
	readonly A a = new A(delegate(int a) { });
	static void Func<T>(TestFunc<T> func) { }
}
