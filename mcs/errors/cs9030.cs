// CS9030: The left-hand side of an assignment cannot contain a null propagating operator
// Line: 11

using System;

class MainClass
{
	public static void Main ()
	{
		System.AppDomain a = null;
		a?.AssemblyLoad += (sender, args) => Console.Write (args);
	}
}