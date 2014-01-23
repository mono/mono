// CS0019: Operator `&&' cannot be applied to operands of type `S?' and `S?'
// Line: 20

struct S
{
	public static S operator & (S s, S i)
	{
		return s;
	}
}

class C
{
	public static void Main ()
	{
		S? s = new S ();
		S? s2 = null;

		var res = s && s2;
	}
}