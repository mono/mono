// test for bug #57151

using System;
using System.Runtime.InteropServices;

namespace Test {
	[StructLayout(LayoutKind.Explicit)]
	struct foo1 {
		public static int foo;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	struct foo2 {
		public static int foo;
		[FieldOffset(0)] public int value;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	class foo3 {
		public static int foo;
		[FieldOffset(0)] public int value;
	}
	
	class Tests {
		public static void Main () {
		}
	}
}
