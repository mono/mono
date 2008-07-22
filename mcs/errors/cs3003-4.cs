// CS3003: Type of `I.this[bool]' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public interface I {
        ulong this[bool index] { get; }
}
