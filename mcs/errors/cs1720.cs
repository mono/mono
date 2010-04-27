// CS1720: Expression will always cause a `System.NullReferenceException'
// Line: 11
// Compiler options: -warnaserror -warn:1

using System;

public class Tester 
{
    public static void Main()
    {
        Console.WriteLine(((object)null).ToString());
    }
}