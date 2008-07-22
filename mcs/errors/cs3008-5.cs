// CS3008: Identifier `I._()' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public interface I {
        void _();
}
