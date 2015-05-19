// CS0138: A `using' directive can only be applied to namespaces but `System.Console' denotes a type. Consider using a `using static' instead
// Line: 5

using System;
using System.Console;

class A
{
	static void Main ()
	{
		Console.WriteLine ("Test CS0138");
	}
}
