//
// System.Numerics.BigInteger
//
// Rodrigo Kumpera (rkumpera@novell.com)

//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

/*
Optimization
	Have proper popcount function for IsPowerOfTwo
	Use unsafe ops to avoid bounds check

*/
namespace System.Numerics {
	public struct BigInteger
	{
		//LSB on [0]
		readonly uint[] data;
		readonly short sign;


		public BigInteger (int value)
		{
			if (value == 0) {
				sign = 0;
				data = new uint [1];
			} else if (value > 0) {
				sign = 1;
				data = new uint[] { (uint) value };
			} else {
				sign = -1;
				data = new uint[1];
				if (value == int.MinValue)
					data [0] = 0x80000000u;
				else
					 data [0] = (uint)-value;
			}
		}

		public BigInteger (long value)
		{
			if (value == 0) {
				sign = 0;
				data = new uint [1];
			} else if (value > 0) {
				sign = 1;
				uint low = (uint)value;
				uint high = (uint)(value >> 32);

				data = new uint [high != 0 ? 2 : 1];
				data [0] = low;
				if (high != 0)
					data [1] = high;
			} else {
				sign = -1;

				if (value == long.MinValue) {
					data = new uint [2] { 0, 0x80000000u };
				} else {
					value = -value;
					uint low = (uint)value;
					uint high = (uint)((ulong)value >> 32);

					data = new uint [high != 0 ? 2 : 1];
					data [0] = low;
					if (high != 0)
						data [1] = high;
				}
			}			
		}

		public BigInteger (byte[] value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			int len = value.Length;

			if (len == 0 || (len == 1 && value [0] == 0)) {
				sign = 0;
				data = new uint [1];
				return;
			}

			if ((value [len - 1] & 0x80) != 0)
				sign = -1;
			else
				sign = 1;

			if (sign == 1) {
				while (value [len - 1] == 0)
					--len;

				int full_words, size;
				full_words = size = len / 4;
				if ((len & 0x3) != 0)
					++size;

				data = new uint [size];
				int j = 0;
				for (int i = 0; i < full_words; ++i) {
					data [i] =	(uint)value [j++] |
								(uint)(value [j++] << 8) |
								(uint)(value [j++] << 16) |
								(uint)(value [j++] << 24);
				}
				size = len & 0x3;
				if (size > 0) {
					int idx = data.Length - 1;
					for (int i = 0; i < size; ++i)
						data [idx] |= (uint)(value [j++] << (i * 8));
				}
			} else {
				int full_words, size;
				full_words = size = len / 4;
				if ((len & 0x3) != 0)
					++size;

				data = new uint [size];

				uint word, borrow = 1;
				ulong sub = 0;
				int j = 0;

				for (int i = 0; i < full_words; ++i) {
					word =	(uint)value [j++] |
							(uint)(value [j++] << 8) |
							(uint)(value [j++] << 16) |
							(uint)(value [j++] << 24);

					sub = (ulong)word - borrow;
					word = (uint)sub;
					borrow = (uint)(sub >> 32) & 0x1u;
					data [i] = ~word;
				}
				size = len & 0x3;

				if (size > 0) {
					word = 0;
					uint store_mask = 0;
					for (int i = 0; i < size; ++i) {
						word |= (uint)(value [j++] << (i * 8));
						store_mask = (store_mask << 8) | 0xFF;
					}

					sub = word - borrow;
					word = (uint)sub;
					borrow = (uint)(sub >> 32) & 0x1u;

					data [data.Length - 1] = ~word & store_mask;
				}
				if (borrow != 0) //FIXME I believe this can't happen, can someone write a test for it?
					throw new Exception ("non zero final carry");
			}

		}

		public bool IsEven {
			get { return (data [0] & 0x1) == 0; }
		}		

		public bool IsOne {
			get { return sign == 1 && data.Length == 1 && data [0] == 1; }
		}		


		//Gem from Hacker's Delight
		//Returns the number of bits set in @x
		static int PopulationCount (uint x)
		{
			x = x - ((x >> 1) & 0x55555555);
			x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
			x = (x + (x >> 4)) & 0x0F0F0F0F;
			x = x + (x >> 8);
			x = x + (x >> 16);
			return (int)(x & 0x0000003F);
		}

		public bool IsPowerOfTwo {
			get {
				bool foundBit = false;
				if (sign != 1)
					return false;
				//This function is pop count == 1 for positive numbers
				for (int i = 0; i < data.Length; ++i) {
					int p = PopulationCount (data [i]);
					if (p > 0) {
						if (p > 1 || foundBit)
							return false;
						foundBit = true;
					}
				}
				return foundBit;
			}
		}		

		public bool IsZero {
			get { return sign == 0; }
		}		

		public int Sign {
			get { return sign; }
		}

		public static explicit operator int (BigInteger value)
		{
			if (value.data.Length > 1)
				throw new OverflowException ();
			uint data = value.data [0];

			if (value.sign == 1) {
				if (data > (uint)int.MaxValue)
					throw new OverflowException ();
				return (int)data;
			} else if (value.sign == -1) {
				if (data > 0x80000000u)
					throw new OverflowException ();
				if (data == 0x80000000u)
					return int.MinValue;
				return -(int)data;
			}

			return 0;
		}

		public static explicit operator long (BigInteger value)
		{
			if (value.sign == 0)
				return 0;

			if (value.data.Length > 2)
				throw new OverflowException ();

			uint low = value.data [0];

			if (value.data.Length == 1) {
				if (value.sign == 1)
					return (long)low;
				long res = (long)low;
				return -res;
			}

			uint high = value.data [1];

			if (value.sign == 1) {
				if (high >= 0x80000000u)
					throw new OverflowException ();
				return (((long)high) << 32) | low;
			}

			if (high > 0x80000000u)
				throw new OverflowException ();

			if (high == 0x80000000u) {
				if (low != 0)
					throw new OverflowException ();
				return long.MinValue;
			}

			return - ((((long)high) << 32) | (long)low);
		}

		public static implicit operator BigInteger (int value)
		{
			return new BigInteger (value);
		}

		public static implicit operator BigInteger (long value)
		{
			return new BigInteger (value);
		}

		static int TopByte (uint x)
		{
			if ((x & 0xFFFF0000u) != 0) {
				if ((x & 0xFF000000u) != 0)
					return 4;
				return 3;
			}
			if ((x & 0xFF00u) != 0)
				return 2;
			return 1;	
		}

		static int FirstNonFFByte (uint word)
		{
			if ((word & 0xFF000000u) != 0xFF000000u)
				return 4;
			else if ((word & 0xFF0000u) != 0xFF0000u)
				return 3;
			else if ((word & 0xFF00u) != 0xFF00u)
				return 2;
			return 1;
		}

		public byte[] ToByteArray ()
		{
			if (sign == 0)
				return new byte [1];

			//number of bytes not counting upper word
			int bytes = (data.Length - 1) * 4;
			bool needExtraZero = false;

			uint topWord = data [data.Length - 1];
			int extra;

			//if the topmost bit is set we need an extra 
			if (sign == 1) {
				extra = TopByte (topWord);
				uint mask = 0x80u << ((extra - 1) * 8);
				if ((topWord & mask) != 0) {
					needExtraZero = true;
				}
			} else {
				extra = TopByte (topWord);
			}

			byte[] res = new byte [bytes + extra + (needExtraZero ? 1 : 0) ];
			if (sign == 1) {
				int j = 0;
				int end = data.Length - 1;
				for (int i = 0; i < end; ++i) {
					uint word = data [i];

					res [j++] = (byte)word;
					res [j++] = (byte)(word >> 8);
					res [j++] = (byte)(word >> 16);
					res [j++] = (byte)(word >> 24);
				}
				while (extra-- > 0) {
					res [j++] = (byte)topWord;
					topWord >>= 8;
				}
			} else {
				int j = 0;
				int end = data.Length - 1;

				uint carry = 1, word;
				ulong add;
				for (int i = 0; i < end; ++i) {
					word = data [i];
					add = (ulong)~word + carry;
					word = (uint)add;
					carry = (uint)(add >> 32);

					res [j++] = (byte)word;
					res [j++] = (byte)(word >> 8);
					res [j++] = (byte)(word >> 16);
					res [j++] = (byte)(word >> 24);
				}

				add = (ulong)~topWord + (carry);
				word = (uint)add;
				carry = (uint)(add >> 32);
				if (carry == 0) {
					int ex = FirstNonFFByte (word);
					bool needExtra = (word & (1 << (ex * 8 - 1))) == 0;
					int to = ex + (needExtra ? 1 : 0);

					if (to != extra)
						res = Resize (res, bytes + to);

					while (ex-- > 0) {
						res [j++] = (byte)word;
						word >>= 8;
					}
					if (needExtra)
						res [j++] = 0xFF;
				} else {
					res = Resize (res, bytes + 5);
					res [j++] = (byte)word;
					res [j++] = (byte)(word >> 8);
					res [j++] = (byte)(word >> 16);
					res [j++] = (byte)(word >> 24);
					res [j++] = 0xFF;
				}
			}

			return res;
		}

		static byte[] Resize (byte[] v, int len)
		{
	        byte[] res = new byte [len];
	       	Array.Copy (v, res, Math.Min (v.Length, len));
	        return res;
	    }
	}
}
