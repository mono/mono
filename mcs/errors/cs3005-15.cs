// cs3005-15.cs: Identifier `a' differing only in case is not CLS-compliant
// Compiler options: -warnaserror
// Line: 10

using System;
[assembly:CLSCompliant (true)]

public enum A {
}

public interface a {
}
