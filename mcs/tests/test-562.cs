using System;
using System.Reflection;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("foo.dll")]
    public static extern void printf(string format, __arglist);

    public static int Main()
    {
        if (typeof (Program).GetMethod ("printf").CallingConvention != CallingConventions.VarArgs)
            return 1;
        
        Console.WriteLine ("OK");
        return 0;
    }
}