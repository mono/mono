// cs0156.cs: A throw statement with no arguments is not allowed outside of a catch clause
// Line: 12

using System;

class Foo
{
	static void Main ()
	{
		try {
			Console.WriteLine ("Test cs0156");
			throw;
		}
		catch {
		}			
	}
}
