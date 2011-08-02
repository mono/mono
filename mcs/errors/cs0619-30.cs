// CS0619-30: `ObsoleteEnum.value_B' is obsolete: `Is obsolete'
// Line: 16

using System;

enum ObsoleteEnum
{
    value_A,
    [Obsolete("Is obsolete", true)]
    value_B
}

class MainClass {
    public static void Main ()
    {
        Console.WriteLine (ObsoleteEnum.value_B);
    }
}