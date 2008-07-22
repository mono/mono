// CS3009: `CLSClass': base type `System.Runtime.Serialization.Formatter' is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror -warn:1

using System;
using System.Runtime.Serialization;

[assembly:CLSCompliant (true)]

public abstract class CLSClass: Formatter {
}
