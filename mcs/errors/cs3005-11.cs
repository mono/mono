// CS3005: Identifier `clsInterface' differing only in case is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant (true)]

public interface CLSInterface {
}

public class clsInterface: CLSInterface {
}
