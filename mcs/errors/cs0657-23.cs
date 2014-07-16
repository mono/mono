// CS0657: `field' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `param'. All attributes in this section will be ignored
// Line: 9
// Compiler options: -warnaserror

using System;

public class FieldAttribute : System.Attribute
{
}

class X ([field:FieldAttribute] int foo)
{
	int v = foo;
}
