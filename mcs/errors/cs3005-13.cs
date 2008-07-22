// CS3005: Identifier `CLSEnum.label' differing only in case is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

public enum CLSEnum {
        label,
        Label
}
