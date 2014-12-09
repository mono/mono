// CS8079: Use of possibly unassigned auto-implemented property `X'
// Line: 11

public struct S
{
	public int X { get; set; }
	public int Y;

	public S ()
	{
		Y = X;
		X = 0;
	}
}