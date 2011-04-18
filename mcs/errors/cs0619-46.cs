// CS0619-46: `C' is obsolete: `Is obsolete'
// Line: 16

using System;

[Obsolete("Is obsolete", true)]
class C
{
	public static string SS;
}

class MainClass
{
    public static void Main ()
    {
        Console.WriteLine (C.SS);
    }
}
