// CS0619-34: `Error' is obsolete: `Obsolete struct'
// Line: 17

using System;

[Obsolete ("Obsolete struct", true)]
struct Error
{
    public static void Report (bool arg)
    {
    }
}

class MainClass {
    public static void Main ()
    {
        Error.Report (false);
    }
}