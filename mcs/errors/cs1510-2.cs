// cs1510-2.cs: an lvalue is required for ref or out argument
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