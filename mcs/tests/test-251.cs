//
// Tests the valid value types for volatile fields.
//

using System;

interface R {
}

enum XX {
	A
}

class X {
	volatile byte a;
	volatile sbyte b;
	volatile short c;
	volatile ushort d;
	volatile int e;
	volatile uint f;
	volatile char g;
	volatile float h;
	volatile bool i;
	volatile X x;
	volatile R r;
	volatile XX dd;
	volatile IntPtr ip;
	volatile UIntPtr uip;

	public static void Main () {}
}
