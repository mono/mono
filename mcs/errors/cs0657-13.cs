// CS0657: `type' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `property'. All attributes in this section will be ignored
// Line: 14
// Compiler options: -warnaserror

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