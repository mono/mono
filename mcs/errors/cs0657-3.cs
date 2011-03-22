// CS0657: `method' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `assembly, module'. All attributes in this section will be ignored
// Line : 7
// Compiler options: -warnaserror

using System;

[method: CLSCompliant (false)]

public class C
{
}
