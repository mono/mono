// cs3003-3.cs: Type of `I.Error' is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant(true)]

public interface I {
        UIntPtr Error { get; }
}
