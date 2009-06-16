// CS0122: `MainClass.Nested.Delegate()' is inaccessible due to its protection level
// Line: 18

delegate int TestDelegate ();

public class MainClass
{
	class Nested
	{
		static int Delegate ()
		{
			return 0;
		}
	}

	public static void Main ()
	{
		TestDelegate delegateInstance = new TestDelegate (Nested.Delegate);
	}
}

