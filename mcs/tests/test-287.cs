using System;
using System.Reflection;

static class StaticClass
{
    const int Foo = 1;    
    
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
        
        if (type.GetConstructors ().Length > 0) {
            Console.WriteLine ("Has constructor");
            return 2;
        }
        
        Console.WriteLine (StaticClass.Name ());
        return 0;
    }
}
