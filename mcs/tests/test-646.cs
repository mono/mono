using System;

// Undocumented IntPtr and UIntPtr conversion exceptions

class Program
{
	static long CastUIntPtrToInt64 (UIntPtr ptr)
	{
		return (long) ptr;
	}
	
	static uint CastIntPtrToUInt32 (IntPtr ptr)
	{
		return (uint) ptr;
	}
	
	public static int Main ()
	{
		if (IntPtr.Size < 8) {
			if (CastUIntPtrToInt64 (new UIntPtr (uint.MaxValue)) != uint.MaxValue)
				return 1;
			if (CastIntPtrToUInt32 (new IntPtr (int.MaxValue)) != int.MaxValue)
				return 2;
		} else {
			if (CastUIntPtrToInt64 (new UIntPtr (ulong.MaxValue)) != -1)
				return 3;
				
			if (CastIntPtrToUInt32 (new IntPtr (long.MaxValue)) != uint.MaxValue)
				return 4;
		}
		
		return 0;
	}
}
