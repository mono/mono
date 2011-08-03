// CS0657: `param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `method, return'. All attributes in this section will be ignored
// Line: 9
// Compiler options: -warnaserror

using System;

struct S
{
    [param: Obsolete]
    void method () {}
}