using System;
using System.Collections.Generic;

class X
{
    internal static Dictionary<Type, Func<string, string>> Test = new Dictionary<Type, Func<string, string>> { {
            typeof (int),
            metadata => "1"
        }, {
            typeof (uint),
            metadata => "2"
        },
    };

    public static void Main ()
    {
    }
}