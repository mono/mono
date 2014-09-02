// CS0429: Unreachable expression code detected
// Line: 24
// Compiler options: -warnaserror

using System;

struct S
{
}

class C
{
	public static implicit operator S (C c)
	{
		return new S ();
	}
}

class Program
{
	static void Main ()
	{
		C c = new C ();
		Console.WriteLine (c ?? new S ());
	}
}
