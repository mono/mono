// cs3005-21.cs: Identifier `CLSClass.NameAbC' differing only in case is not CLS-compliant
// Compiler options: -warnaserror
// Line: 12

using System;
[assembly:CLSCompliant (true)]

public class CLSClass {
        [CLSCompliant (false)]
        public int NameABC;
            
        public int NameAbc;
        public int NameAbC;
}
