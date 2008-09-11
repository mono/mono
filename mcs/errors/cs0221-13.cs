// CS0221: Constant value `NaN' cannot be converted to a `char' (use `unchecked' syntax to override)
// Line: 6

class X {
	static void Main () {
		System.Console.WriteLine ((char)float.NaN);
	}
}
