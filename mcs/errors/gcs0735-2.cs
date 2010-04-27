// gcs0735-2.cs: Invalid type specified as an argument for TypeForwardedTo attribute
// Line: 7

using System;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof (int[]))]