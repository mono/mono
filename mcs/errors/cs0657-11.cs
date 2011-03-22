// CS0657: `param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `method, return'. All attributes in this section will be ignored
// Line: 15
// Compiler options: -warnaserror

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