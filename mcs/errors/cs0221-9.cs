// CS0221: Constant value `3.402823E+38' cannot be converted to a `ulong' (use `unchecked' syntax to override)
// Line: 6

class X {
	static void Main () {
		const float d = float.MaxValue;
		ulong b = (ulong) d;
	}
}
