// CS0156: A throw statement with no arguments is not allowed outside of a catch clause
// Line: 14

using System;

class Foo
{
	static void Main ()
	{
		try {
		    Console.WriteLine ("TEST");
		}
		finally {
			throw;
		}
	}
}
