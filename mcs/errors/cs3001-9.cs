// cs3001.cs: Argument type 'ulong' is not CLS-compliant
// Line: 13

using System;
[assembly:CLSCompliant (A.f)]

public class A
{
    public const bool f = true;
}

public interface I {
        void Test (ulong arg);
}
