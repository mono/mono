// cs0077.cs: As operator can only be used with reference types.
// Line: 10

using System;

class ErrorCS0077 {
	struct Foo { }
	public static void Main () {
		Foo s1, s2; 
		s1 = s2 as ErrorCS0077.Foo;
	}
}

