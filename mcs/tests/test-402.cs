// Compiler options: -unsafe

using System;

unsafe class X {
	public static int Main () {
		int y = 20;
		byte* x = (byte*)0;
		x += (long)y;
		// x == 20;
		return (int)x - 20 * sizeof (byte);
	}
}
