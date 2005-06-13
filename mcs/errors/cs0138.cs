// cs0138.cs: A using namespace directive can only be applied to namespaces; `System.Console' is a type not a namespace
// Line: 5

using System;
using System.Console;

class A
{
	static void Main ()
	{
		Console.WriteLine ("Test cs0138");
	}
}
