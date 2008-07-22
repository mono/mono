// CS3001: Argument type `sbyte' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        public delegate int MyDelegate(sbyte i);
}
