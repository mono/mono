// CS0034: Operator `+' is ambiguous on operands of type `A' and `A'
// Line: 22

public class A {
	public static implicit operator int (A a)
	{
		return 0;
	}

	public static implicit operator string (A a)
	{
		return "a";
	}

	public static void Main ()
	{
		A a = new A ();
		object o = a + a;
		System.Console.WriteLine (o);
	}
}
