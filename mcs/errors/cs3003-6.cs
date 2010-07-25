// CS3003: Type of `I.Foo' is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (true)]
public interface I {
    uint Foo { set; }
}

