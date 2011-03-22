// CS0657: `assembly' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `field'. All attributes in this section will be ignored
// Line: 9
// Compiler options: -warnaserror

using System;

public enum E
{
    [assembly: CLSCompliant (false)]
    item
}
