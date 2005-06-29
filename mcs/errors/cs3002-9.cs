// cs3002-9.cs: Return type of `Delegate' is not CLS-compliant
// Line: 7

using System;
[assembly:CLSCompliant(true)]

public delegate CLSDelegate Delegate ();
    
[Serializable]
[CLSCompliant (false)]
public class CLSDelegate {
}