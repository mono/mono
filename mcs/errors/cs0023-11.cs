// cs0023: The `-' operator cannot be applied to operand of type `A'
// Line: 16


class A
{
	public static implicit operator ulong (A mask)
	{
		return 8;
	}
}

class X
{
    static A a = null;
    static object o = -a;
}
