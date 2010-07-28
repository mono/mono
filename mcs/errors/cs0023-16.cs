// CS0023: The `.' operator cannot be applied to operand of type `method group'
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
