// CS0201: Only assignment, call, increment, decrement, and new object expressions can be used as a statement
// Line: 10

class TestClass
{
	delegate void test_delegate (int arg);

	public TestClass ()
	{
		test_delegate D = (b) => "a";
	}
}

