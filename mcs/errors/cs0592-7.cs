// CS0592: The attribute `EnumAttribute' is not valid on this declaration type. It is valid on `method' declarations only
// Line: 13

using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public class EnumAttribute : Attribute {}

public enum E
{
        e_1,
        [EnumAttribute]
        e_2
}
