// cs3005-11.cs: Identifier `clsInterface' differing only in case is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant (true)]

public interface CLSInterface {
}

public class clsInterface: CLSInterface {
}
