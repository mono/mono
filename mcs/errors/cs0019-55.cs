// CS0019: Operator `==' cannot be applied to operands of type `A' and `int?'
// Line: 22

class A
{
	public static bool operator == (A a, int b)
	{
		return false;
	}
	
	public static bool operator != (A a, int b)
	{
		return false;
	}
}

class C
{
	public static void Main ()
	{
		A a = new A ();
		object b = a == Id;
	}
	
	static int? Id {
		get { return 1; }
	}
}
