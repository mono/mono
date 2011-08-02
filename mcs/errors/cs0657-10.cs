// CS0657: `param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `field'. All attributes in this section will be ignored
// Line: 7
// Compiler options: -warnaserror

struct S
{
    [param: Obsolete]
    int field;
}