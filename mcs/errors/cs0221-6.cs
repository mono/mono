// cs0221-6.cs: Constant value `NaN' cannot be converted to a `int' (use `unchecked' syntax to override)
// Line: 6

class X {
	static void Main () {
		System.Console.WriteLine ((int)double.NaN);
	}
}
