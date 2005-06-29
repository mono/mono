// cs3003-4.cs: Type of `I.this[bool]' is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant(true)]

public interface I {
        ulong this[bool index] { get; }
}
