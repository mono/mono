// Compiler options: -warnaserror -warn:4

using System;

public class Test
{
    [Obsolete]
    static void Method () {}
        
    public static void Main ()
    {
        #pragma warning disable 219, 612
        int i = 0;
        Method ();
        #pragma warning restore 612
        #pragma warning disable
        Method ();
        #pragma warning restore
    }
}
