// CS0625: Instance field of type marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute.

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
