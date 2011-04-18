// CS0637: The FieldOffset attribute is not allowed on static or const fields
// Line: 10

using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
struct GValue {
	[FieldOffset (4)]
        public static int value = 3;
}