// TODO: add test of no constructor presence in the class when mono will support it

using System;
using System.Reflection;

static class StaticClass
{
    public static string Name ()
    {
        return "OK";
    }
}

public class MainClass
{
    public static int Main ()
    {
        Type type = typeof (StaticClass);
        if (!type.IsAbstract || !type.IsSealed) {
            Console.WriteLine ("Is not abstract sealed");
            return 1;
        }
        
        Console.WriteLine (StaticClass.Name ());
        return 0;
    }
}
