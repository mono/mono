// gcs0733.cs: Cannot forward generic type `C<int>'
// Line: 8
// Compiler options: -r:GCS0733-lib.dll

using System;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof (C<int>))]
