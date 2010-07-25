// CS0035: Operator `-' is ambiguous on an operand of type `A'
// Line: 22

class A
{
	public static implicit operator float(A x)
	{
		return 0;
	}

	public static implicit operator decimal(A x)
	{
		return 0;
	}
}

class M
{
	static void Main()
	{
		A a = new A ();
		float f = -a;  
	}
}
