// cs0200: can not assign to property X.P -- it is readonly
// line: 12
class X {
	static int P {
		get {
			return 1;
		}
	}

	static int Main ()
	{
		P = 10;
		return 1;
	}
}
