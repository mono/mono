// CS0657: `property' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `event'. All attributes in this section will be ignored
// Line: 9
// Compiler options: -warnaserror

using System;

class C
{
    [property: Obsolete]
    event ResolveEventHandler field { 
        add {}
        remove {}
    }
}
