delegate void TestFunc (int val);

class A
{
	public A(TestFunc func)
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
