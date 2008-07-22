// CS3014: `I.Valid(bool)' cannot be marked as CLS-compliant because the assembly is not marked as CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;

public interface I {
        [CLSCompliant (true)]
        void Valid (bool arg);
}
