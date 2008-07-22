// CS3003: Type of `I.Error' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public interface I {
        UIntPtr Error { get; }
}
