// CS1540: Cannot access protected member `A.A(int)' via a qualifier of type `A'. The qualifier must be of type `B' or derived from it
// Line: 25

public class A
{
	protected A (int a)
	{
	}
}

public class B : A
{
	public B ()
		: base (1)
	{
	}
	
	public static void Main ()
	{
		A a = new A (1);
	}
}
