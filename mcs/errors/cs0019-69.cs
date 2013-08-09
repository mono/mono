// CS0019: Operator `==' cannot be applied to operands of type `S' and `S'
// Line: 22

struct S
{
	public static implicit operator E (S s)
	{
		return 0;
	}
}

public enum E
{
}

class C
{
	public static void Main ()
	{
		S s;
		S s2;
		var x = s == s2;
	}
}