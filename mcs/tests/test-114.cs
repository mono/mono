using System;

class MyClass {

	delegate bool IsAnything (Char c);

	public static int Main () {
		IsAnything validDigit;
		validDigit = new IsAnything (Char.IsDigit);

		return 0;
	}
}
