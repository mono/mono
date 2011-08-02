// CS0619-33: `Error.Report(string)' is obsolete: `Obsolete method'
// Line: 11
// Compiler options: -reference:CS0619-33-lib.dll

using System;

class MainClass {
    public static void Main ()
    {
        Error e = new Error ();
        e.Report ("text");
    }
}