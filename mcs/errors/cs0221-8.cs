// cs0221-8.cs: Constant value `Infinity' cannot be converted to a `uint' (use `unchecked' syntax to override)
// Line: 6

class X {
	static void Main () {
		System.Console.WriteLine ((uint)double.PositiveInfinity);
	}
}
