// cs3009.cs: 'CLSClass': base type 'BaseClass' is not CLS-compliant
// Line: 11

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (false)]
public class BaseClass {
}

public class CLSClass: BaseClass {
}
