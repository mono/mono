// CS3005: Identifier `II.foo' differing only in case is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant(true)]

public interface II {
        int Foo();
        int foo { get; }
}
