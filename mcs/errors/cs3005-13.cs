// cs3005-13.cs: Identifier `CLSEnum.Label' differing only in case is not CLS-compliant
// Line: 9

using System;
[assembly:CLSCompliant (true)]

public enum CLSEnum {
        label,
        Label
}