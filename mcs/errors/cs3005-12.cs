// cs3005.cs: Identifier 'CLSClass.NameAbC(int)' differing only in case is not CLS-compliant
// Line: 9

using System;
[assembly:CLSCompliant (true)]

public class CLSClass {
        public int NameABC;
        public static void NameAbC(int arg) {}
}