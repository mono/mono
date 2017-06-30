// CS8133: Cannot deconstruct dynamic objects
// Line: 9

class C
{
	public static void Test (dynamic d)
	{
		int x, y;
		(x, y) = d;
	}
}