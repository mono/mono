// cs0657.cs : 'param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are 'event, property'
// Line : 8

using System;

class C
{
    [param: Obsolete]
    event ResolveEventHandler field { 
        add {}
        remove {}
    }
}