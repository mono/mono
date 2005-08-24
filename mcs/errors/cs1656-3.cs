// cs1656-3.cs: Cannot assign to `i' because it is a `foreach iteration variable'
// Line: 9

class X {

	static void Main ()
	{
		foreach (int i in new int[] { 2, 3 }) {
		    i = 4;
		}
	}
}
	
