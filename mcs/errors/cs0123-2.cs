// CS0123: The method `MainClass.Delegate()' parameters do not match delegate `IA TestDelegate(bool)' parameters
// Line: 17

delegate IA TestDelegate(bool b);

interface IA {}

public class MainClass : IA
{
	static MainClass Delegate()
	{
		return null;
	}

	public static void Main()
	{
		TestDelegate delegateInstance = new TestDelegate (Delegate);
	}
}

