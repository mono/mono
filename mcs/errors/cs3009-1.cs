// cs3009.cs: 'Days': base type 'uint' is not CLS-compliant
// Line: 7

using System;
[assembly:CLSCompliant(true)]

public enum Days: uint {Sat=1, Sun, Mon, Tue, Wed, Thu, Fri};
