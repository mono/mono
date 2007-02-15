// CS1533: Invoke cannot be called directly on a delegate
// Line : 11
// Compiler options: -langversion:ISO-1

public class TestClass
{
	delegate void OneDelegate (int i);

	static void Main()
	{
		OneDelegate d = new OneDelegate (TestMethod);
		d.Invoke (1);
	}
	public static void TestMethod (int i)
	{
	}
}
