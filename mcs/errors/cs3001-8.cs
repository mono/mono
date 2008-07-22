// CS3001: Argument type `int*' is not CLS-compliant
// Line: 9
// Compiler options: -unsafe -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

unsafe public abstract class CLSClass {
        public void Method (int* param) {}
}
