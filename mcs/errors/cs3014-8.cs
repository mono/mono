// CS3014: `E2.Foo' cannot be marked as CLS-compliant because the assembly is not marked as CLS-compliant
// Line: 8
// Compiler options: -warnaserror -warn:1

using System;

public enum E2 {
        [CLSCompliant (true)]
        Foo
}
