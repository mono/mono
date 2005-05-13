// cs1656.cs: Cannot assign to 'i' because it is a 'foreach iteration variable'
// line: 9

class X {

	static void Main ()
	{
		foreach (int i in new int[] { 2, 3 }) {
		    i = 4;
		}
	}
}
	
