// CS0120: An object reference is required to access non-static member `Foo.Bar()'
// Line: 22

using System;

public class Foo
{
	public string Bar ()
	{
		return "hello";
	}
	public static string Bar (string thing)
	{
		return string.Format ("hello {0}", thing);
	}
}

public class MainClass
{
	public static void Main ()
	{
		Console.WriteLine (Foo.Bar ());
	}
}
