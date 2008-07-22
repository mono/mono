// CS3008: Identifier `CLSClass._value' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant(true)]

public class CLSClass {
        public const string _value = "";
}
