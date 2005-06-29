// cs0657-10.cs: `param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `field'
// Line : 6

struct S
{
    [param: Obsolete]
    int field;
}