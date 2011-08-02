// CS0657: `type' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `param'. All attributes in this section will be ignored
// Line: 15
// Compiler options: -warnaserror

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