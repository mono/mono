// cs3005-12.cs: Identifier `CLSClass.NameAbC(int)' differing only in case is not CLS-compliant
// Compiler options: -warnaserror
// Line: 9

using System;
[assembly:CLSCompliant (true)]

public class CLSClass {
        public int NameABC;
        public static void NameAbC(int arg) {}
}
