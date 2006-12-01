// CS0035 : Operator `+' is ambiguous on operands of type `Y' and `X'
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
		int i = -a;  
	}
}