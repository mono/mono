// cs3005-13.cs: Identifier `CLSEnum.label' differing only in case is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror

//
// NOTE: This is only an error in MCS - in GMCS, it's just a warning.
//

using System;
[assembly:CLSCompliant (true)]

public enum CLSEnum {
        label,
        Label
}
