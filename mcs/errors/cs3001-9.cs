// CS3001: Argument type `ulong' is not CLS-compliant
// Line: 14
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (A.f)]

public class A
{
    public const bool f = true;
}

public interface I {
        void Test (ulong arg);
}
