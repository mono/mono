// cs3002.cs: Return type of 'CLSClass.Foo()' is not CLS-compliant
// Line: 12

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