// Compiler options: -unsafe

using System;
using System.Runtime.InteropServices;

namespace Bug
{
	[StructLayout (LayoutKind.Sequential, Pack = 1)]
	public unsafe struct xxx
	{
		internal fixed byte zzz [5];
	}

	internal class RedSkyTimeCode
	{
		public unsafe void CopyTo (xxx* dest)
		{
			fixed (ulong* p = &_rep) {
				byte* pb = (byte*) p;
				dest->zzz [0] = pb [0];
				dest->zzz [1] = pb [1];
				dest->zzz [2] = pb [2];
				dest->zzz [3] = pb [3];
				dest->zzz [4] = pb [4];
			}
		}
		
		public static unsafe void Convert (xxx* src, ulong* dest)
		{
			byte* pb = (byte*) dest;
			*dest = 0L;
			pb [0] = src->zzz [0];
			pb [1] = src->zzz [1];
			pb [2] = src->zzz [2];
			pb [3] = src->zzz [3];
			pb [4] = src->zzz [4];
		}
		
		private ulong _rep;
		
		public static void Main ()
		{
		}
	}
}

