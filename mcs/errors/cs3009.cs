// CS3009: `CLSClass': base type `BaseClass' is not CLS-compliant
// Line: 12
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (false)]
public class BaseClass {
}

public class CLSClass: BaseClass {
}
