// cs1016.cs: Named attribute argument expected
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