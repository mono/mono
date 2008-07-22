// CS3002: Return type of `CLSClass.Foo()' is not CLS-compliant
// Line: 14
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        [CLSCompliant(false)]
        public ulong Valid() {
                return 1;
        }
    
        protected internal ulong Foo() {
                return 1;
        }
       
        static void Main() {}
}
