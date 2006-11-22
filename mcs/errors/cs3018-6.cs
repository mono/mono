// CS3018: `I.Error()' cannot be marked as CLS-compliant because it is a member of non CLS-compliant type `I'
// Line: 11
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

[CLSCompliant(false)]
public interface I {
        [CLSCompliant(true)]
        ulong[] Error();
}