// Compiler options: -r:test-883-lib.dll -t:library

public enum E
{
	TestField = 3
}

public class Second
{
	public void TestFinal ()
	{
		TestClass.Foo (E.TestField);
	}
}
