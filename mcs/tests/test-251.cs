//
// Tests the valid value types for volatile fields.
//
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

	static void Main () {}
}
