// cs1502-6.cs: The best overloaded method match for `Test.Test(TestMethod2)' has some invalid arguments
// Line: 8

public class Test
{
	void Foo ()
	{
		new Test (new TestMethod (Foo));
	}

	public Test (TestMethod2 test) {}
}

public delegate void TestMethod ();
public delegate void TestMethod2 (object o);