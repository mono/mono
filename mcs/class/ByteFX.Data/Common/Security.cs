using System;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for Security.
	/// </summary>
	internal class Security
	{
		public Security()
		{
		}

		public static void ArrayCrypt( byte[] src, int srcoff, byte[] dst, int dstoff, byte[] key, int length )
		{
			int idx = 0;

			while ( (idx+srcoff) < src.Length && idx < length )
			{
				dst[idx+dstoff] = (byte)(src[idx+srcoff] ^ key[idx]);
				idx++;
			}
		}
	}
}
