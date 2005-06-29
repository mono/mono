// cs3015-2.cs: `CLSAttribute' has no accessible constructors which use only CLS-compliant types
// Line: 7

using System;
[assembly:CLSCompliant (true)]

public class CLSAttribute: Attribute {
    [CLSCompliant (false)]
    public CLSAttribute(string array) {
    }
}
