// cs0625-3.cs: `cs0625.GValue.value': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute
// Line: 10

using System;
using System.Runtime.InteropServices;

namespace cs0625 {
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
