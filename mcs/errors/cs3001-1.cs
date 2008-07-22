// CS3001: Argument type `ulong' is not CLS-compliant
// Line: 8
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

public delegate long MyDelegate (ulong arg);
