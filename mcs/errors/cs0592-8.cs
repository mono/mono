// CS0592: The attribute `System.ObsoleteAttribute' is not valid on this declaration type. It is valid on `class, struct, enum, constructor, method, property, indexer, field, event, interface, delegate' declarations only
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
