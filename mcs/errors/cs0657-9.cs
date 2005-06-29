// cs0657-9.cs: `type' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `param'
// Line : 13

using System;

[AttributeUsage(AttributeTargets.All)]
public class TestAttribute: Attribute
{
}

struct S
{
    void method (
        [type: Test]
        int p) {}
}