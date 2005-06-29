// cs0657-14.cs: `param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `event, field, method'
// Line : 8

using System;

struct S
{
    [param: Obsolete]
    event ResolveEventHandler field;
}