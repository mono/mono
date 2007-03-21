// cs3005-15.cs: Identifier `a' differing only in case is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant (true)]

public enum A {
}

public interface a {
}
