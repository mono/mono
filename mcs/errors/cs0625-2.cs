// CS0625: `CS0625.GValue.foo': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute
// Line: 10

using System;
using System.Runtime.InteropServices;

namespace CS0625 {
	[StructLayout(LayoutKind.Explicit)]
	partial class GValue {
		public int foo;
	}
	
	class Tests {
		public static void Main () {
		}
	}
}
