// Compiler options: -r:gtest-301-lib.dll

using System;

public class Test
{
    [STAThread]
    public static void Main (string[] args)
    {
        IShape shape;

        object[] cargs = new object[1] { "Circle" };
        shape = Factory<IShape>.CreateInstance (cargs);    
    }

    interface IShape
    {
    }
}

