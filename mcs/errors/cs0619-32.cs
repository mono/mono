// CS0619-32: `E.item_a' is obsolete: `Obsolete enum'
// Line: 10
// Compiler options: -reference:CS0619-32-lib.dll

using System;

class MainClass {
    public static void Main ()
    {
        Console.WriteLine (E.item_a);
    }
}