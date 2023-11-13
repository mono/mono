using System;
using System.IO;

public class Program
{
    private static void Main ()
    {
        var content = File.ReadAllText ("hello-io.cs");
        Console.WriteLine (content);
    }
}