// cs1706.cs: Anonymous methods are not allowed in the attribute declaration
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

