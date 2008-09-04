// CS0031: Constant value `1000M' cannot be converted to a `byte'
// Line: 8

class C
{
	public static void Main ()
	{
		const byte c = unchecked ((byte) 1000M);
	}
}

