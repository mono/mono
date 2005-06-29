// cs0657-16.cs: `event' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `method, param, return'
// Line: 14

using System;

[AttributeUsage(AttributeTargets.All)]
public class TestAttribute: Attribute
{
}

class C
{
    event ResolveEventHandler field { 
        [event: Test]
        add {}
        remove {}
            
    }
}