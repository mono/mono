// cs3003.cs: Type of 'S.test2' is not CLS-compliant
// Line: 11
// Compiler options: -unsafe

using System;

[assembly: CLSCompliant (true)]

public unsafe struct S
{
    public fixed bool test2 [4];
}