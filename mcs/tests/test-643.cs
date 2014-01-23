// Compiler options: -unsafe

using System;

class PointerArithmeticTest
{
	unsafe public static int Main()
	{
		try {
			return CheckAdd((byte*)(-1), -1);
		} catch (System.OverflowException) {}
		
		try {
			return CheckSub((short*)(-1), int.MaxValue);
		} catch (System.OverflowException) {}
		
		CheckSub2((short*)(-1), int.MaxValue);
			
		if ((long)Conversions (long.MaxValue) != (IntPtr.Size <= 4 ? uint.MaxValue : long.MaxValue))
			return 5;
		
		Console.WriteLine ("OK");
		return 0;
	}
	
	unsafe static int* Conversions (long b)
	{
		return (int*)b;
	}
	
	unsafe static int CheckAdd(byte* ptr, int offset)
	{
		if (checked(ptr + offset < ptr))
			return 1;
		
		return 101;
	}
	
	unsafe static int CheckSub(short* ptr, int offset)
	{
		if (checked(ptr - offset < ptr))
			return 2;

		return 102;
	}

	unsafe static int CheckSub2(short* ptr, int offset)
	{
		short* b = ptr + offset;
		if (checked(ptr - b < 0))
			return 3;

		return 0;
	}
}
