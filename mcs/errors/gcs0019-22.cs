// CS0019: Operator `==' cannot be applied to operands of type `int' and `S?'
// Line: 15

struct S
{
	public static bool operator !=(int a, S s) { return true; }
	public static bool operator ==(int a, S s) { return false; }
}

public class C
{
	public static void Main ()
	{
		S? s;
		var b = 1 == s;
	}
}
