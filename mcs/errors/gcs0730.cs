// CS0730: Cannot forward type `C.CC' because it is a nested type
// Line: 8
// Compiler options: -r:GCS0730-lib.dll

using System;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof (C.CC))]
