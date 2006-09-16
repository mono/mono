// CS3003: Type of `I.Foo' is not CLS-compliant
// Line: 9

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (true)]
public interface I {
    uint Foo { set; }
}

