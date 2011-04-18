// CS0165: Use of unassigned local variable `service'
// Line: 17
using System;

public class Foo
{
	static void Main (string[] args)
	{
		int service;

		int pos = 0;
		while (pos < args.Length) {
			service = 1;
			break;
		}

		Console.WriteLine (service);
	}
}
