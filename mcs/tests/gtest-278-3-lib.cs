// Compiler options: -r:gtest-278-2-lib.dll -t:library

using System;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo (typeof (C))]
[assembly: TypeForwardedTo (typeof (D))]
[assembly: TypeForwardedTo (typeof (G<int>))]
