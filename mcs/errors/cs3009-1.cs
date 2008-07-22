// CS3009: `Days': base type `uint' is not CLS-compliant
// Line: 8
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public enum Days: uint {Sat=1, Sun, Mon, Tue, Wed, Thu, Fri};
