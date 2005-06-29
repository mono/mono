// cs0657-4.cs: `param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `assembly, module'
// Line : 6

using System;

[param: CLSCompliant (false)]
public enum E
{
    item
}
