// CS0742: A throw statement with no argument is only allowed in a catch clause nested inside of the innermost catch clause
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
