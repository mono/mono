// cs3001.cs: Argument type 'int*' is not CLS-compliant
// Line: 9
// Compiler options: --unsafe

using System;
[assembly:CLSCompliant(true)]

unsafe public abstract class CLSClass {
        public void Method (int* param) {}
}
