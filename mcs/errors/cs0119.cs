// cs0119.cs: 'Test.E()' is a 'method', which is not valid in the given context
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
