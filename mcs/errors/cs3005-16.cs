// cs3005.cs: Identifier 'CLSClass_B.uNIVERSAL' differing only in case is not CLS-compliant
// Line: 9
// Compiler options: -reference:cs3005-16-lib.dll

using System;
[assembly: CLSCompliantAttribute (true)]

public class CLSClass_B: CLSClass_A {
        public const int uNIVERSAL = 3;
}