// cs3001.cs: Argument type 'sbyte' is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        public delegate int MyDelegate(sbyte i);
}