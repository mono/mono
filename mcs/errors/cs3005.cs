// CS3005: Identifier `ErrorCS3005.FOO' differing only in case is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant (true)]

public class ErrorCS3005 {
	public int FOO = 0;
	public int foo = 1;

	public static void Main ( ) {
		ErrorCS3005 error = new ErrorCS3005 ();
		Console.WriteLine ("This should make the compiler to complain ERROR CS3005, number: {0}", error.foo);
	}
}

