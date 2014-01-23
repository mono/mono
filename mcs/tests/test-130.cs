//
// Check casts.
//
using System;

class X {

	public const short a = 128;
	public const int b = 0xffff;
	public const double c = 123.4;

	public const long d = 5;
	// public const int e = 2147483648;

	public const byte f = 127;

	public const char c1 = (char) 0xffff;
	public const char c2 = (char) 123.4;
	public const char c3 = (char) a;
	public const char c4 = (char) b;
	public const char c5 = (char) c;

	public const short s2 = (short) c;
	public IntPtr p = (IntPtr) null;
	public static int Main ()
	{
		return 0;
	}
}
