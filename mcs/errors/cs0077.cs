// cs0077.cs: The as operator must be used with a reference type (`ErrorCS0077.Foo' is a value type)
// Line: 10

using System;

class ErrorCS0077 {
	struct Foo { }
	public static void Main () {
		Foo s1, s2; 
		s1 = s2 as ErrorCS0077.Foo;
	}
}

