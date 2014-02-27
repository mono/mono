// CS0121: The call is ambiguous between the following methods or properties: `C.M(int, string, string)' and `C.M<int>(int, int?, string)'
// Line: 16

class C
{
	static void M (int x, string y, string z = null)
	{
	}

	static void M<T>(T t, int? u, string z = null)
	{
	}

	static void Main ()
	{
		M (123, null);
	}
}