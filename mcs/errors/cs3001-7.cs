// CS3001: Argument type `ulong' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public interface I {
        long this[ulong indexA] { set; }
}
