// CS0200: Property or indexer `A.X' cannot be assigned to (it is read-only)
// Line: 13

public class A
{
	public int X { get; }
}

public class B : A
{
	public B ()
	{
		base.X = 1;
	}
}