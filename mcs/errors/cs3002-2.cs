// cs3002-2.cs: Return type of 'CLSClass.Foo()' is not CLS-compliant
// Line: 13

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