// cs3005: Identifier foo differing only in case is not CLS-Compliant.
// Line: 13 


using System;

class ErrorCS3005 {
	public int FOO = 0;
	public int foo = 1;

	public static void Main ( ) {
		ErrorCS3005 error = new ErrorCS3005 ();
		Console.WriteLine ("This should make the compiler to complain ERROR CS3005, number: {0}", error.foo);
	}
}

