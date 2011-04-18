// CS0019: Operator `-' cannot be applied to operands of type `string' and `ulong'
// Line : 12

using System;

public class C
{
    public static void Main ()
    {
        ulong aa = 10;
        ulong bb = 3;
        Console.WriteLine("bug here --> "+aa-bb);
    }
}
