// cs0037-4.cs: Cannot convert null to `int' because it is a value type
// Line: 9

class X {
	static void Main ()
	{
		int s = 44;
		switch (s) {
			case null: break;
		}
	}
}
