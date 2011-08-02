// CS0619-31: `ObsoleteEnum' is obsolete: `Is obsolete'
// Line: 15

using System;

[Obsolete("Is obsolete", true)]
enum ObsoleteEnum
{
    value_B
}

class MainClass {
    public static void Main ()
    {
        Console.WriteLine (ObsoleteEnum.value_B);
    }
}