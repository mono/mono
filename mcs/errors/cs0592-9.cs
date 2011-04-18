// CS0592: The attribute `SAttribute' is not valid on this declaration type. It is valid on `return' declarations only
// Line : 12

using System;

[AttributeUsage (AttributeTargets.ReturnValue)]
class SAttribute: Attribute {}

public class C
{
    int Prop {
        [param: S]
        set {
        }
    }
}
