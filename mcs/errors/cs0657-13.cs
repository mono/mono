// cs0657-13.cs: `type' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `property'
// Line: 13

using System;

[AttributeUsage(AttributeTargets.All)]
public class TestAttribute: Attribute
{
}

class C
{
    [type:Test]
    int Prop {
        set {
        }
    }
}