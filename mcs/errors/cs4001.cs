// CS4001: Cannot await `void' expression
// Line: 10

using System;

class A
{
	static async void Test ()
	{
		await Console.WriteLine ("await");
	}
}
