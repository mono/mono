// CS0123: A method or delegate `TestDelegateA(bool)' parameters do not match delegate `TestDelegateB(int)' parameters
// Line: 12

delegate int TestDelegateA (bool b);
delegate int TestDelegateB (int b);

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

