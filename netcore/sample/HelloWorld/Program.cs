using System;
using System.Runtime.CompilerServices;

public class Program
{
    static void Main()
    {
        dynamic a = new int[] {1};
        Console.WriteLine(a.Foo1() + " (False)");
        Console.WriteLine(a.Foo2() + " (False)");
        Console.WriteLine(a.Foo3() + " (True)");
        Console.WriteLine(a.Foo4() + " (True)");
    }
}
