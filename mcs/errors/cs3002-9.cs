// CS3002: Return type of `Delegate' is not CLS-compliant
// Line: 8
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public delegate CLSDelegate Delegate ();
    
[Serializable]
[CLSCompliant (false)]
public class CLSDelegate {
}
