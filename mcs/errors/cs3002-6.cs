// cs3002-6.cs: Return type of 'I.Error()' is not CLS-compliant
// Line: 10

using System;
[assembly:CLSCompliant(true)]

[CLSCompliant(false)]
public interface I {
        [CLSCompliant(true)]
        ulong[] Error();
}