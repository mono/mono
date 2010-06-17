enum E { a, b }

class C
{
	public static void Main ()
	{
		M ((uint) 0);
		M ((long) 0);
		M ((sbyte) 0);
		M ((ulong) 0);
	}

	static void M (E e)
	{
	}
}