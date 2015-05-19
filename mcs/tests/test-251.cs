// Compiler options: -unsafe
//
// Tests the valid value types for volatile fields.
//

using System;

interface R {
}

enum XX {
	A
}

struct S {
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
	unsafe volatile ushort* uc;
	unsafe volatile XX* udd;
	unsafe volatile S* us;

	public static void Main () {}
}
