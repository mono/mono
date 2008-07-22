// CS3014: `E' cannot be marked as CLS-compliant because the assembly is not marked as CLS-compliant
// Line: 8
// Compiler options: -warnaserror -warn:1

using System;

[CLSCompliant (true)]
public enum E {
}
