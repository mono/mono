// CS3008: Identifier `C._()' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public class C {
        public void _() {}
}
