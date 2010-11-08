// CS0843: An automatically implemented property `S.A' must be fully assigned before control leaves the constructor. Consider calling the default struct contructor from a constructor initializer
// Line: 8

public struct S
{
	public int A { get; set;}

	public S (int a)
	{
		this.A = a;
	}
}
