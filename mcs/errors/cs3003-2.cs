// CS3003: Type of `CLSClass.Index' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        public ulong Index {
            set
            {
            }
        }
}
