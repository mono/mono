// cs0657.cs : 'param' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are 'method, return'
// Line : 4

struct S
{

    [param: Obsolete]
    void method () {}
}