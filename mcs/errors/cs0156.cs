// CS0156: A throw statement with no argument is only allowed in a catch clause
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
