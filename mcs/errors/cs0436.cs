// CS0436: The type `System.Console' conflicts with the imported type of same name'. Ignoring the imported type definition
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
