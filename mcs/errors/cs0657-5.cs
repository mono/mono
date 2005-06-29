// cs0657-5.cs: `assembly' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `field'
// Line : 7

using System;

public enum E
{
    [assembly: CLSCompliant (false)]
    item
}
