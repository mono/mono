// cs0637-2.cs: The FieldOffset attribute is not allowed on static or const fields
// Line: 10

using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
struct GValue {
	[FieldOffset (4)]
        public const int value = 3;
}