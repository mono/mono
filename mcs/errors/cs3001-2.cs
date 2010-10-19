// CS3001: Argument type `ulong' is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

public class CLSClass {
        public CLSClass (long a) {}
        public CLSClass (ref ulong a) {}
}
