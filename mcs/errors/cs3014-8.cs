// cs3014-8.cs: 'E2.Foo' cannot be marked as CLS compliant because the assembly is not marked as compliant
// Line: 7

using System;

public enum E2 {
        [CLSCompliant (true)]
        Foo
}
