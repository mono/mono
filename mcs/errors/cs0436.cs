// cs0436.cs: Ignoring imported type `System.Console' since there exists a declaration with the same name
// Line: 16
// Compiler options: -warn:2 -warnaserror

namespace System
{
    public class Console
    {
    }
}

public class C
{
    public static void Main ()
    {
        new System.Console ();
    }
}
