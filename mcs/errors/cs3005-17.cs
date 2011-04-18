// CS3005: Identifier `CLSClass.op_implicit()' differing only in case is not CLS-compliant
// Line: 12
// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        static public implicit operator CLSClass(byte value) {
               return new CLSClass();
        }
    
        public void op_implicit () {}
}
