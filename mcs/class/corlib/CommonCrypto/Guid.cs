using Crimson.CommonCrypto;

namespace System
{
	partial struct Guid
	{
		public static Guid NewGuid ()
		{
			byte[] b = new byte [16];
			Cryptor.GetRandom (b);

			Guid res = new Guid (b);
			// Mask in Variant 1-0 in Bit[7..6]
			res._d = (byte) ((res._d & 0x3fu) | 0x80u);
			// Mask in Version 4 (random based Guid) in Bits[15..13]
			res._c = (short) ((res._c & 0x0fffu) | 0x4000u);

			return res;
		}
	}
}
