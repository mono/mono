// cs3009.cs: 'CLSClass': base type 'System.Runtime.Serialization.Formatter' is not CLS-compliant
// Line: 9

using System;
using System.Runtime.Serialization;

[assembly:CLSCompliant (true)]

public abstract class CLSClass: Formatter {
}
