// CS0410: A method or delegate `MainClass MainClass.Delegate()' parameters and return type must be same as delegate `IA TestDelegate()' parameters and return type
// Line: 18
// Compiler options: -langversion:ISO-1

delegate IA TestDelegate();

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

