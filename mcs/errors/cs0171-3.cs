// CS0171: Field `Test.x' must be fully assigned before control leaves the constructor
// Line: 10

public struct Test
{
	internal int x;

	public Test (int x)
	{
		X = x;
	}

	public int X
	{
		get { return x; }
		set { x = value; }
	}
}
