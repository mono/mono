// CS1656: Cannot assign to `i' because it is a `foreach iteration variable'
// Line: 9

class Test
{
	static void Main ()
	{
		foreach (int i in new int[] { 1, 2})
			i++;
	}
}
