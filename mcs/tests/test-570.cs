//Compiler options: -warnaserror -warn:4

using System;
interface IFoo
{
}

class Bar
{
}

class Program
{
	public static void Main()
	{
		IFoo foo = null;
		if (foo is IFoo)
			Console.WriteLine("got an IFoo"); // never prints
			
		Bar bar = null;
		if (bar is Bar)
			Console.WriteLine("got a bar"); // never prints
	}
}
