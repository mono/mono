// cs3001.cs: Argument type 'ulong' is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant(true)]

public interface I {
        long this[ulong indexA] { set; }
}
