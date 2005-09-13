// cs3005-8.cs: Identifier `II.Foo()' differing only in case is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant(true)]

public interface II {
        int Foo();
        int foo { get; }
}
