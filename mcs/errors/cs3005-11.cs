// cs3005-11.cs: Identifier `clsInterface' differing only in case is not CLS-compliant
// Compiler options: -warnaserror
// Line: 10

using System;
[assembly:CLSCompliant (true)]

public interface CLSInterface {
}

public class clsInterface: CLSInterface {
}
