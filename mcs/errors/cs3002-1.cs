// cs3002-2.cs: Return type of 'CLSClass.Foo()' is not CLS-compliant
// Line: 13

using System;
[assembly:CLSCompliant(true)]

[CLSCompliant(false)]
public interface I1 {
}

public class CLSClass {
        protected internal I1 Foo() {
                return null;
        }
       
        static void Main() {}
}