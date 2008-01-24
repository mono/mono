// CS0077: The `as' operator cannot be used with a non-nullable value type `ErrorCS0077.Foo'
// Line: 10

using System;

class ErrorCS0077 {
	struct Foo { }
	public static void Main () {
		Foo s1, s2; 
		s1 = s2 as ErrorCS0077.Foo;
	}
}

