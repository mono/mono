// CS0625: `CS0625.GValue.name': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute
// Line: 11

using System;
using System.Runtime.InteropServices;

namespace CS0625 {
	[StructLayout(LayoutKind.Explicit)]
	struct GValue {
		public string name;
		[ FieldOffset (4) ] public int value;
	}
	
	class Tests {
		public static void Main () {
		}
	}
}
