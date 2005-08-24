// cs0625-1.cs: `cs0625.GValue.foo': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute
// Line: 10

using System;
using System.Runtime.InteropServices;

namespace cs0625 {
	[StructLayout(LayoutKind.Explicit)]
	class GValue {
		public int foo;
	}
	
	class Tests {
		public static void Main () {
		}
	}
}
