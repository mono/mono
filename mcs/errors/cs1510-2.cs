// CS1510: A ref or out argument must be an assignable variable
// Line: 14
// this is bug #56016

using System;

class Test {
	static void test(ref IConvertible i) {
	}
	
	static void Main() {
		int i = 1;

		test (ref (IConvertible) i);
	}
}