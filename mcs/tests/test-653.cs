// Compiler options: -unsafe 
using System;

class C
{
	static unsafe int Test ()
	{
		try {
			uint* i = stackalloc uint[int.MaxValue];
			uint v = 0;
			i [v] = v;
			i [0] = v;
			return 1;
		} catch (OverflowException) {
			return 0;
		}
	}
	
	unsafe static void Test2 ()
	{
		byte* b = null;
		b = b + (byte)1;
		b = b + (sbyte)1;
		b = b + (short)1;
		b = b + (int)1;
		b = b + (long)1;
		b = b + (ulong)1;
	}
	
	unsafe static void Test2 (sbyte sb, short s, int i, long l, ulong ul)
	{
		short* b = null;
		b = b + sb;
		b = b + s;
		b = b + i;
		b = b + l;
		b = b + ul;
	}
	
	public static int Main ()
	{
		Test2 ();
		Test2 (1, 2, 3, 4, 5);
		if (Test () != 0)
			return 1;
			
		Console.WriteLine ("OK");
		return 0;
	}
}
