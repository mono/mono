// cs0119.cs: Expression denotes a `method group', where a `variable', `value' or `type' was expected
// Line: 14

using System;

public class Test
{
    public static void E () 
    { 
    }

    public static void Main () 
    {
        Console.WriteLine(E.x);
    }
}
