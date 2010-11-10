// CS0019: Operator `==' cannot be applied to operands of type `S?' and `int'
// Line: 15

struct S
{
	public static bool operator != (S s, int a) { return true; }
	public static bool operator == (S s, int a) { return false; }
}

public class C
{
	public static void Main ()
	{
		S? s;
		var b = s == 1;
	}
}
