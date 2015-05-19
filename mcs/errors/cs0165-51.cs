// CS0165: Use of unassigned local variable `s'
// Line: 12
// Compiler options: -r:CS0165-51-lib.dll

using System;

class C<T> where T : class
{
	public static void Foo ()
	{
	    S<T> s;
	    Console.WriteLine (s);
	}
}
