// CS0200: Property or indexer `C.S' cannot be assigned to (it is read-only)
// Line: 10

class C
{
	public static int S { get; }

	public C ()
	{
		S = 3;
	}
}
