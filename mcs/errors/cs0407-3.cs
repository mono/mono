// CS0407: A method or delegate `int TestDelegateA(bool)' return type does not match delegate `bool TestDelegateB(bool)' return type
// Line: 12

delegate int TestDelegateA (bool b);
delegate bool TestDelegateB (bool b);

public class MainClass
{
	public static int Delegate(bool b)
	{
		return 0;
	}

	public static void Main() 
	{
		TestDelegateA a = new TestDelegateA (Delegate);
		TestDelegateB b = new TestDelegateB (a);
	}
}

