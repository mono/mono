// CS0121: The call is ambiguous between the following methods or properties: `B.operator +(A, B)' and `A.operator +(A, B)'
// Line: 21

class A
{
	public static A operator + (A a, B b)
	{
		return null;
	}
}

class B
{
	public static A operator + (A a, B b)
	{
		return null;
	}

	static void Main ()
	{
		object o = new A () + new B ();
	}
}
