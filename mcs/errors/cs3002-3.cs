// cs3002.cs: Return type of 'CLSClass.MyDelegate' is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        public delegate uint MyDelegate();
}