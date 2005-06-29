// cs0657-15.cs: `property' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `event'
// Line : 8

using System;

class C
{
    [property: Obsolete]
    event ResolveEventHandler field { 
        add {}
        remove {}
    }
}
