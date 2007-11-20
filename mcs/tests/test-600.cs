using System;

namespace Test
{
	enum Key_byte : byte { A = 1}
	enum Key_ulong : ulong { A = 1}

	class Regression
	{
		public static int Main()
		{
			IntPtr a = new IntPtr (1);
			UIntPtr b = new UIntPtr (1);
			Key_byte k1 = (Key_byte)a;
			Key_byte k2 = (Key_byte)b;
			
			if (k1 != Key_byte.A)
				return 1;
				
			if (k2 != Key_byte.A)
				return 2;
				
			Key_ulong k1_u = (Key_ulong)a;
			Key_ulong k2_u = (Key_ulong)b;

			if (k1_u != Key_ulong.A)
				return 1;
				
			if (k2_u != Key_ulong.A)
				return 2;

			return 0;
		}
	}
}
