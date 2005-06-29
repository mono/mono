// cs0657-11.cs: `param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `method, return'
// Line: 14

using System;

[AttributeUsage(AttributeTargets.All)]
public class TestAttribute: Attribute
{
}

class C
{
    int Prop {
        [param:Test]
        get {
            return 1;
        }
    }
}