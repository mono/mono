// CS0123: A method or delegate `MainClass.Delegate()' parameters do not match delegate `TestDelegate(bool)' parameters
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

