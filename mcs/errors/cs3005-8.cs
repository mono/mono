// cs3005.cs: Identifier 'II.foo' differing only in case is not CLS-compliant
// Line: 9

using System;
[assembly:CLSCompliant(true)]

public interface II {
        int Foo();
        int foo { get; }
}