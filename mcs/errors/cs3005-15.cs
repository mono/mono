// cs3005.cs: Identifier 'a' differing only in case is not CLS-compliant
// Line: 10

using System;
[assembly:CLSCompliant (true)]

public enum A {
}

public interface a {
}