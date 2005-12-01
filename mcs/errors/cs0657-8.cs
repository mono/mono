// cs0657-8.cs: `param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `method, return'
// Line : 9

using System;

struct S
{

    [param: Obsolete]
    void method () {}
}