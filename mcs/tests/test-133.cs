using System;

public struct S {
	public int a, b;
}

class T {
	enum OpCode : ushort { False }
	enum OpFlags : ushort { None }
	static void DecodeOp (ushort word, out OpCode op, out OpFlags flags) {
		op = (OpCode)(word & 0x00ff);
		flags = (OpFlags)(word & 0xff00);
	}
	static void get_struct (out S s) {
		S ss;
		ss.a = 1;
		ss.b = 2;
		s = ss;
	}
	public static int Main() {
		OpCode op;
		OpFlags flags;
		S s;
		DecodeOp ((ushort)0x0203, out op, out flags);
		if (op != (OpCode)0x3)
			return 1;
		if (flags != (OpFlags)0x200)
			return 2;
		get_struct (out s);
		if (s.a != 1)
			return 3;
		if (s.b != 2)
			return 4;
		return 0;
	}
}
