// CS3009: `C': base type `C<ulong>' is not CLS-compliant
// Line: 14
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public class C<T>
{
}

public class C : C<ulong>
{
}
