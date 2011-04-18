// CS0729: Cannot forward type `C' because it is defined in this assembly
// Line: 7

using System;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof (C))]

class C
{
}