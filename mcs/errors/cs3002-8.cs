// cs3002-8.cs: Return type of 'I.Error' is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant(true)]

public interface I {
        ulong this[bool index] { get; }
}
