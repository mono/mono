// CS3009: `AttributesForm': base type `BaseClass' is not CLS-compliant
// Line: 17
// Compiler options: -warnaserror -warn:1


// The error is reported intentionally twice.

using System;

[assembly:CLSCompliant(true)]

[CLSCompliant(false)]
public class BaseClass
{
}

public class AttributesForm : BaseClass
{
}

public class AttributesForm_2 : BaseClass
{
}
