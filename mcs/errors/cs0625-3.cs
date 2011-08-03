// CS0625: `CS0625.GValue.value': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute
// Line: 10

using System;
using System.Runtime.InteropServices;

namespace CS0625 {
	[StructLayout (LayoutKind.Explicit)]
	partial struct GValue
	{
	}
	
	partial struct GValue {
		public int value;
	}
	
	class Tests {
		public static void Main () {
		}
	}
}
