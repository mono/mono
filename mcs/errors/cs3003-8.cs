// CS3003: Type of `S.test2' is not CLS-compliant
// Line: 11
// Compiler options: -unsafe -warnaserror -warn:1

using System;

[assembly: CLSCompliant (true)]

public unsafe struct S
{
    public fixed bool test2 [4];
}
