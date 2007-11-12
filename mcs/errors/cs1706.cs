// CS1706: Anonymous methods and lambda expressions cannot be used in the current context
// Line: 13

using System;

delegate void TestDelegate();

class MyAttr : Attribute
{
    public MyAttr (TestDelegate d) { }
}

[MyAttr (delegate {} )]
class C
{
}

