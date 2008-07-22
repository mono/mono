// CS3002: Return type of `CLSClass.Foo()' is not CLS-compliant
// Line: 13
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        private ulong Valid() {
                return 1;
        }
    
        protected ulong Foo() {
                return 1;
        }
}
