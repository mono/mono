// cs3005-24.cs: Identifier `ClsClass' differing only in case is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror

[assembly:System.CLSCompliant(true)]

public partial class CLSClass {}

public partial struct ClsClass {}
