// CS0200: Property or indexer `C.P' cannot be assigned to (it is read-only)
// Line: 10

class C
{
	public int P { get; }

	public void Foo ()
	{
		P = 10;
	}
}
