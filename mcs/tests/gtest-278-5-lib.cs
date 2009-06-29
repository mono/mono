// Compiler options: -t:library -r:gtest-278-4-lib.dll -out:gtest-278-2-lib.dll

using System;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo (typeof (C))]
[assembly: TypeForwardedTo (typeof (D))]
[assembly: TypeForwardedTo (typeof (G<int>))]
