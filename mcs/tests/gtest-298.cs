delegate void TestFunc<T> (T val);

class A
{
	public A (TestFunc<int> func)
	{
		func (0);
	}
}

class TestClass
{
	static int i = 1;
	static readonly A a = new A(delegate(int a) { i = a; });

	public static int Main ()
	{
		return i;
	}
}
