// cs3001.cs: Argument type 'ulong' is not CLS-compliant
// Line: 7

using System;
[assembly:CLSCompliant (true)]

public delegate long MyDelegate (ulong arg);
