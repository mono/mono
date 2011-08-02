// CS0636: The FieldOffset attribute can only be placed on members of types marked with the StructLayout(LayoutKind.Explicit)
// Line: 10

using System;
using System.Runtime.InteropServices;

namespace CS0636 {
	class GValue {
		[FieldOffset (4)] 
                public const int value = 2;
	}
}
