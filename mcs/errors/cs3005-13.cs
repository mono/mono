// CS3005: Identifier `CLSEnum.Label' differing only in case is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

public enum CLSEnum {
        label,
        Label
}
