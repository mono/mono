// cs0657.cs : Attribute 'Obsolete' is not valid on this declaration type. It is valid on 'class, struct, enum, constructor, method, property, field, event, interface, delegate' declarations only.
// Line : 8

using System;

public class C
{
    [return: Obsolete]
    public void Test (int a)
    {
    }
    static public void Main () {}
}
