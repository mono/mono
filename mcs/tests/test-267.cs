using System;
 
public class X
{
    [CLSCompliant (false)]
    public static string Text ()
    {
        return "PASS";
    }
   
    public static int Main ()
    {
        Console.WriteLine (Text ());
        return 0;
    }
}
