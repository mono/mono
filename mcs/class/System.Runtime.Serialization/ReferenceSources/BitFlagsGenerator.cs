using System;

namespace System.Runtime.Serialization
{
	class BitFlagsGenerator
	{
		int bitCount;
		byte [] locals;
		
		public BitFlagsGenerator (int bitCount)
		{
			this.bitCount = bitCount;
			int localCount = (bitCount+7)/8;
			locals = new byte [localCount];
		}
		
		public void Store (int bitIndex, bool value)
		{
			if (value)
				locals [GetByteIndex (bitIndex)] |= GetBitValue(bitIndex);
			else
				locals [GetByteIndex (bitIndex)] &= (byte) ~GetBitValue(bitIndex);
		}
		
		public bool Load (int bitIndex)
		{
			var local = locals[GetByteIndex(bitIndex)];
			byte bitValue = GetBitValue(bitIndex);
			return (local & bitValue) == bitValue;
		}
		
		public byte [] LoadArray ()
		{
			return (byte []) locals.Clone ();
		}
		
		public int GetLocalCount ()
		{
			return locals.Length;
		}
		
		public int GetBitCount ()
		{
			return bitCount;
		}
		
		public byte GetLocal (int i)
		{
			return locals [i];
		}
		
		public static bool IsBitSet (byte[] bytes, int bitIndex)
		{
			int byteIndex = GetByteIndex (bitIndex);
			byte bitValue = GetBitValue (bitIndex);
			return (bytes[byteIndex] & bitValue) == bitValue;
		}

		public static void SetBit (byte[] bytes, int bitIndex)
		{
			int byteIndex = GetByteIndex (bitIndex);
			byte bitValue = GetBitValue (bitIndex);
			bytes[byteIndex] |= bitValue;
		}

		static int GetByteIndex (int bitIndex)
		{
			return bitIndex >> 3;
		}
		
		static byte GetBitValue (int bitIndex)
		{
			return (byte)(1 << (bitIndex & 7));
		}
	}
}
