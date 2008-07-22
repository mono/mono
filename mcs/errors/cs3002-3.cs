// CS3002: Return type of `CLSClass.MyDelegate' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        public delegate uint MyDelegate();
}
