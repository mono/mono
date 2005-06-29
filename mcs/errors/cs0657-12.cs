// cs0657-12.cs: `type' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `method, param, return'
// Line: 14

using System;

[AttributeUsage(AttributeTargets.All)]
public class TestAttribute: Attribute
{
}

class C
{
    int Prop {
        [type:Test]
        set {
        }
    }
}