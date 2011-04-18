// CS0457: Ambiguous user defined conversions `A.implicit operator byte(A)' and `A.implicit operator sbyte(A)' when converting from 'A' to 'int'
// Line: 20

class A
{
	public static implicit operator ushort (A mask)
	{
		return 1;
	}

	public static implicit operator short (A mask)
	{
		return 2;
	}
}

class X
{
    static A a = null;
    static object o = -a;
}
