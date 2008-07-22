// CS3008: Identifier `CLSClass._myEvent' is not CLS-compliant
// Line: 11
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant(true)]

public delegate void MyDelegate();

public class CLSClass {
        public event MyDelegate _myEvent;
}
