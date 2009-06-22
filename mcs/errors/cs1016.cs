// CS1016: Named attribute arguments must appear after the positional arguments
// Line: 19

using System;
using System.Runtime.CompilerServices;

class Attr: Attribute
{
    public Attr (int i) {}
    
    public string Arg {
        set {}
        get { return "a"; }
    }
}

public class E
{
    [Attr (Arg = "xxx", 3)]
    public void Method () {}
}
