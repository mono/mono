// CS0188: The `this' object cannot be used before all of its fields are assigned to
// Line: 10

public struct Test
{
	private int x;

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
