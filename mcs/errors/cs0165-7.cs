// CS0165: Use of unassigned local variable `x'
// Line: 16
using System;

public class Foo
{
	static void Main (string[] args)
	{
		int service;

		for (int pos = 0; pos < args.Length; pos++) {
			service = 1;
			break;
		}

		Console.WriteLine (service);
	}
}
