// CS0165: Use of unassigned local variable `service'
// Line: 17
using System;

public class Foo
{
	static void Main ()
	{
		int service;

		foreach (char b in "hola") {
			Console.WriteLine (b);
			service = 1;
			break;
		}

		Console.WriteLine (service);
	}
}
