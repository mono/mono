// cs0165.cs: Use of unassigned local variable
// Line: 9

class C {
	public static int test4 ()
	{
		int a;

		try {
			a = 3;
		} catch {
		}

		// CS0165
		return a;
	}
}
