public class TestClass
{
	delegate void OneDelegate (int i);

	public static void Main()
	{
		OneDelegate d = new OneDelegate (TestMethod);
		d.Invoke (1);
	}
	public static void TestMethod (int i)
	{
	}
}
