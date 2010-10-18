//
// BigInteger.cs - Big Integer implementation
//
// Authors:
//	Ben Maurer
//	Chew Keong TAN
//	Sebastien Pouliot <sebastien@ximian.com>
//	Pieter Philippaerts <Pieter@mentalis.org>
//
// Copyright (c) 2003 Ben Maurer
// All rights reserved
//
// Copyright (c) 2002 Chew Keong TAN
// All rights reserved.
//
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
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
using System.Security.Cryptography;
using Mono.Math.Prime.Generator;
using Mono.Math.Prime;

namespace Mono.Math {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class BigInteger {

		#region Data Storage

		/// <summary>
		/// The Length of this BigInteger
		/// </summary>
		uint length = 1;

		/// <summary>
		/// The data for this BigInteger
		/// </summary>
		uint [] data;

		#endregion

		#region Constants

		/// <summary>
		/// Default length of a BigInteger in bytes
		/// </summary>
		const uint DEFAULT_LEN = 20;

		/// <summary>
		///		Table of primes below 2000.
		/// </summary>
		/// <remarks>
		///		<para>
		///		This table was generated using Mathematica 4.1 using the following function:
		///		</para>
		///		<para>
		///			<code>
		///			PrimeTable [x_] := Prime [Range [1, PrimePi [x]]]
		///			PrimeTable [6000]
		///			</code>
		///		</para>
		/// </remarks>
		internal static readonly uint [] smallPrimes = {
			2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71,
			73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151,
			157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233,
			239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317,
			331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419,
			421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503,
			509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607,
			613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701,
			709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811,
			821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911,
			919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997,

			1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087,
			1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181,
			1187, 1193, 1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279,
			1283, 1289, 1291, 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373,
			1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471,
			1481, 1483, 1487, 1489, 1493, 1499, 1511, 1523, 1531, 1543, 1549, 1553, 1559,
			1567, 1571, 1579, 1583, 1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637,
			1657, 1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733, 1741, 1747,
			1753, 1759, 1777, 1783, 1787, 1789, 1801, 1811, 1823, 1831, 1847, 1861, 1867,
			1871, 1873, 1877, 1879, 1889, 1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973,
			1979, 1987, 1993, 1997, 1999, 
		
			2003, 2011, 2017, 2027, 2029, 2039, 2053, 2063, 2069, 2081, 2083, 2087, 2089,
			2099, 2111, 2113, 2129, 2131, 2137, 2141, 2143, 2153, 2161, 2179, 2203, 2207,
			2213, 2221, 2237, 2239, 2243, 2251, 2267, 2269, 2273, 2281, 2287, 2293, 2297,
			2309, 2311, 2333, 2339, 2341, 2347, 2351, 2357, 2371, 2377, 2381, 2383, 2389,
			2393, 2399, 2411, 2417, 2423, 2437, 2441, 2447, 2459, 2467, 2473, 2477, 2503,
			2521, 2531, 2539, 2543, 2549, 2551, 2557, 2579, 2591, 2593, 2609, 2617, 2621,
			2633, 2647, 2657, 2659, 2663, 2671, 2677, 2683, 2687, 2689, 2693, 2699, 2707,
			2711, 2713, 2719, 2729, 2731, 2741, 2749, 2753, 2767, 2777, 2789, 2791, 2797,
			2801, 2803, 2819, 2833, 2837, 2843, 2851, 2857, 2861, 2879, 2887, 2897, 2903,
			2909, 2917, 2927, 2939, 2953, 2957, 2963, 2969, 2971, 2999,
			
			3001, 3011, 3019, 3023, 3037, 3041, 3049, 3061, 3067, 3079, 3083, 3089, 3109,
			3119, 3121, 3137, 3163, 3167, 3169, 3181, 3187, 3191, 3203, 3209, 3217, 3221,
			3229, 3251, 3253, 3257, 3259, 3271, 3299, 3301, 3307, 3313, 3319, 3323, 3329,
			3331, 3343, 3347, 3359, 3361, 3371, 3373, 3389, 3391, 3407, 3413, 3433, 3449,
			3457, 3461, 3463, 3467, 3469, 3491, 3499, 3511, 3517, 3527, 3529, 3533, 3539,
			3541, 3547, 3557, 3559, 3571, 3581, 3583, 3593, 3607, 3613, 3617, 3623, 3631,
			3637, 3643, 3659, 3671, 3673, 3677, 3691, 3697, 3701, 3709, 3719, 3727, 3733,
			3739, 3761, 3767, 3769, 3779, 3793, 3797, 3803, 3821, 3823, 3833, 3847, 3851,
			3853, 3863, 3877, 3881, 3889, 3907, 3911, 3917, 3919, 3923, 3929, 3931, 3943,
			3947, 3967, 3989,
			
			4001, 4003, 4007, 4013, 4019, 4021, 4027, 4049, 4051, 4057, 4073, 4079, 4091,
			4093, 4099, 4111, 4127, 4129, 4133, 4139, 4153, 4157, 4159, 4177, 4201, 4211,
			4217, 4219, 4229, 4231, 4241, 4243, 4253, 4259, 4261, 4271, 4273, 4283, 4289,
			4297, 4327, 4337, 4339, 4349, 4357, 4363, 4373, 4391, 4397, 4409, 4421, 4423,
			4441, 4447, 4451, 4457, 4463, 4481, 4483, 4493, 4507, 4513, 4517, 4519, 4523,
			4547, 4549, 4561, 4567, 4583, 4591, 4597, 4603, 4621, 4637, 4639, 4643, 4649,
			4651, 4657, 4663, 4673, 4679, 4691, 4703, 4721, 4723, 4729, 4733, 4751, 4759,
			4783, 4787, 4789, 4793, 4799, 4801, 4813, 4817, 4831, 4861, 4871, 4877, 4889,
			4903, 4909, 4919, 4931, 4933, 4937, 4943, 4951, 4957, 4967, 4969, 4973, 4987,
			4993, 4999,
			
			5003, 5009, 5011, 5021, 5023, 5039, 5051, 5059, 5077, 5081, 5087, 5099, 5101,
			5107, 5113, 5119, 5147, 5153, 5167, 5171, 5179, 5189, 5197, 5209, 5227, 5231, 
			5233, 5237, 5261, 5273, 5279, 5281, 5297, 5303, 5309, 5323, 5333, 5347, 5351,
			5381, 5387, 5393, 5399, 5407, 5413, 5417, 5419, 5431, 5437, 5441, 5443, 5449,
			5471, 5477, 5479, 5483, 5501, 5503, 5507, 5519, 5521, 5527, 5531, 5557, 5563,
			5569, 5573, 5581, 5591, 5623, 5639, 5641, 5647, 5651, 5653, 5657, 5659, 5669,
			5683, 5689, 5693, 5701, 5711, 5717, 5737, 5741, 5743, 5749, 5779, 5783, 5791,
			5801, 5807, 5813, 5821, 5827, 5839, 5843, 5849, 5851, 5857, 5861, 5867, 5869,
			5879, 5881, 5897, 5903, 5923, 5927, 5939, 5953, 5981, 5987
		};

		public enum Sign : int {
			Negative = -1,
			Zero = 0,
			Positive = 1
		};

		#region Exception Messages
		const string WouldReturnNegVal = "Operation would return a negative value";
		#endregion

		#endregion

		#region Constructors

		public BigInteger ()
		{
			data = new uint [DEFAULT_LEN];
			this.length = DEFAULT_LEN;
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif          
		public BigInteger (Sign sign, uint len) 
		{
			this.data = new uint [len];
			this.length = len;
		}

		public BigInteger (BigInteger bi)
		{
			this.data = (uint [])bi.data.Clone ();
			this.length = bi.length;
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif       
		public BigInteger (BigInteger bi, uint len)
		{

			this.data = new uint [len];

			for (uint i = 0; i < bi.length; i++)
				this.data [i] = bi.data [i];

			this.length = bi.length;
		}

		#endregion

		#region Conversions
		
		public BigInteger (byte [] inData)
		{
			if (inData.Length == 0)
				inData = new byte [1];
			length = (uint)inData.Length >> 2;
			int leftOver = inData.Length & 0x3;

			// length not multiples of 4
			if (leftOver != 0) length++;

			data = new uint [length];

			for (int i = inData.Length - 1, j = 0; i >= 3; i -= 4, j++) {
				data [j] = (uint)(
					(inData [i-3] << (3*8)) |
					(inData [i-2] << (2*8)) |
					(inData [i-1] << (1*8)) |
					(inData [i])
					);
			}

			switch (leftOver) {
			case 1: data [length-1] = (uint)inData [0]; break;
			case 2: data [length-1] = (uint)((inData [0] << 8) | inData [1]); break;
			case 3: data [length-1] = (uint)((inData [0] << 16) | (inData [1] << 8) | inData [2]); break;
			}

			this.Normalize ();
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public BigInteger (uint [] inData)
		{
			if (inData.Length == 0)
				inData = new uint [1];
			length = (uint)inData.Length;

			data = new uint [length];

			for (int i = (int)length - 1, j = 0; i >= 0; i--, j++)
				data [j] = inData [i];

			this.Normalize ();
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public BigInteger (uint ui)
		{
			data = new uint [] {ui};
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public BigInteger (ulong ul)
		{
			data = new uint [2] { (uint)ul, (uint)(ul >> 32)};
			length = 2;

			this.Normalize ();
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public static implicit operator BigInteger (uint value)
		{
			return (new BigInteger (value));
		}

		public static implicit operator BigInteger (int value)
		{
			if (value < 0) throw new ArgumentOutOfRangeException ("value");
			return (new BigInteger ((uint)value));
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public static implicit operator BigInteger (ulong value)
		{
			return (new BigInteger (value));
		}

		/* This is the BigInteger.Parse method I use. This method works
		because BigInteger.ToString returns the input I gave to Parse. */
		public static BigInteger Parse (string number) 
		{
			if (number == null)
				throw new ArgumentNullException ("number");

			int i = 0, len = number.Length;
			char c;
			bool digits_seen = false;
			BigInteger val = new BigInteger (0);
			if (number [i] == '+') {
				i++;
			} 
			else if (number [i] == '-') {
				throw new FormatException (WouldReturnNegVal);
			}

			for (; i < len; i++) {
				c = number [i];
				if (c == '\0') {
					i = len;
					continue;
				}
				if (c >= '0' && c <= '9') {
					val = val * 10 + (c - '0');
					digits_seen = true;
				} 
				else {
					if (Char.IsWhiteSpace (c)) {
						for (i++; i < len; i++) {
							if (!Char.IsWhiteSpace (number [i]))
								throw new FormatException ();
						}
						break;
					} 
					else
						throw new FormatException ();
				}
			}
			if (!digits_seen)
				throw new FormatException ();
			return val;
		}

		#endregion

		#region Operators

		public static BigInteger operator + (BigInteger bi1, BigInteger bi2)
		{
			if (bi1 == 0)
				return new BigInteger (bi2);
			else if (bi2 == 0)
				return new BigInteger (bi1);
			else
				return Kernel.AddSameSign (bi1, bi2);
		}

		public static BigInteger operator - (BigInteger bi1, BigInteger bi2)
		{
			if (bi2 == 0)
				return new BigInteger (bi1);

			if (bi1 == 0)
				throw new ArithmeticException (WouldReturnNegVal);

			switch (Kernel.Compare (bi1, bi2)) {

				case Sign.Zero:
					return 0;

				case Sign.Positive:
					return Kernel.Subtract (bi1, bi2);

				case Sign.Negative:
					throw new ArithmeticException (WouldReturnNegVal);
				default:
					throw new Exception ();
			}
		}

		public static int operator % (BigInteger bi, int i)
		{
			if (i > 0)
				return (int)Kernel.DwordMod (bi, (uint)i);
			else
				return -(int)Kernel.DwordMod (bi, (uint)-i);
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public static uint operator % (BigInteger bi, uint ui)
		{
			return Kernel.DwordMod (bi, (uint)ui);
		}

		public static BigInteger operator % (BigInteger bi1, BigInteger bi2)
		{
			return Kernel.multiByteDivide (bi1, bi2)[1];
		}

		public static BigInteger operator / (BigInteger bi, int i)
		{
			if (i > 0)
				return Kernel.DwordDiv (bi, (uint)i);

			throw new ArithmeticException (WouldReturnNegVal);
		}

		public static BigInteger operator / (BigInteger bi1, BigInteger bi2)
		{
			return Kernel.multiByteDivide (bi1, bi2)[0];
		}

		public static BigInteger operator * (BigInteger bi1, BigInteger bi2)
		{
			if (bi1 == 0 || bi2 == 0) return 0;

			//
			// Validate pointers
			//
			if (bi1.data.Length < bi1.length) throw new IndexOutOfRangeException ("bi1 out of range");
			if (bi2.data.Length < bi2.length) throw new IndexOutOfRangeException ("bi2 out of range");

			BigInteger ret = new BigInteger (Sign.Positive, bi1.length + bi2.length);

			Kernel.Multiply (bi1.data, 0, bi1.length, bi2.data, 0, bi2.length, ret.data, 0);

			ret.Normalize ();
			return ret;
		}

		public static BigInteger operator * (BigInteger bi, int i)
		{
			if (i < 0) throw new ArithmeticException (WouldReturnNegVal);
			if (i == 0) return 0;
			if (i == 1) return new BigInteger (bi);

			return Kernel.MultiplyByDword (bi, (uint)i);
		}

		public static BigInteger operator << (BigInteger bi1, int shiftVal)
		{
			return Kernel.LeftShift (bi1, shiftVal);
		}

		public static BigInteger operator >> (BigInteger bi1, int shiftVal)
		{
			return Kernel.RightShift (bi1, shiftVal);
		}

		#endregion

		#region Friendly names for operators

		// with names suggested by FxCop 1.30

		public static BigInteger Add (BigInteger bi1, BigInteger bi2) 
		{
			return (bi1 + bi2);
		}

		public static BigInteger Subtract (BigInteger bi1, BigInteger bi2) 
		{
			return (bi1 - bi2);
		}

		public static int Modulus (BigInteger bi, int i) 
		{
			return (bi % i);
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public static uint Modulus (BigInteger bi, uint ui) 
		{
			return (bi % ui);
		}

		public static BigInteger Modulus (BigInteger bi1, BigInteger bi2) 
		{
			return (bi1 % bi2);
		}

		public static BigInteger Divid (BigInteger bi, int i) 
		{
			return (bi / i);
		}

		public static BigInteger Divid (BigInteger bi1, BigInteger bi2) 
		{
			return (bi1 / bi2);
		}

		public static BigInteger Multiply (BigInteger bi1, BigInteger bi2) 
		{
			return (bi1 * bi2);
		}

		public static BigInteger Multiply (BigInteger bi, int i) 
		{
			return (bi * i);
		}

		#endregion

		#region Random
		private static RandomNumberGenerator rng;
		private static RandomNumberGenerator Rng {
			get {
				if (rng == null)
					rng = RandomNumberGenerator.Create ();
				return rng;
			}
		}

		/// <summary>
		/// Generates a new, random BigInteger of the specified length.
		/// </summary>
		/// <param name="bits">The number of bits for the new number.</param>
		/// <param name="rng">A random number generator to use to obtain the bits.</param>
		/// <returns>A random number of the specified length.</returns>
		public static BigInteger GenerateRandom (int bits, RandomNumberGenerator rng)
		{
			int dwords = bits >> 5;
			int remBits = bits & 0x1F;

			if (remBits != 0)
				dwords++;

			BigInteger ret = new BigInteger (Sign.Positive, (uint)dwords + 1);
			byte [] random = new byte [dwords << 2];

			rng.GetBytes (random);
			Buffer.BlockCopy (random, 0, ret.data, 0, (int)dwords << 2);

			if (remBits != 0) {
				uint mask = (uint)(0x01 << (remBits-1));
				ret.data [dwords-1] |= mask;

				mask = (uint)(0xFFFFFFFF >> (32 - remBits));
				ret.data [dwords-1] &= mask;
			}
			else
				ret.data [dwords-1] |= 0x80000000;

			ret.Normalize ();
			return ret;
		}

		/// <summary>
		/// Generates a new, random BigInteger of the specified length using the default RNG crypto service provider.
		/// </summary>
		/// <param name="bits">The number of bits for the new number.</param>
		/// <returns>A random number of the specified length.</returns>
		public static BigInteger GenerateRandom (int bits)
		{
			return GenerateRandom (bits, Rng);
		}

		/// <summary>
		/// Randomizes the bits in "this" from the specified RNG.
		/// </summary>
		/// <param name="rng">A RNG.</param>
		public void Randomize (RandomNumberGenerator rng)
		{
			if (this == 0)
				return;

			int bits = this.BitCount ();
			int dwords = bits >> 5;
			int remBits = bits & 0x1F;

			if (remBits != 0)
				dwords++;

			byte [] random = new byte [dwords << 2];

			rng.GetBytes (random);
			Buffer.BlockCopy (random, 0, data, 0, (int)dwords << 2);

			if (remBits != 0) {
				uint mask = (uint)(0x01 << (remBits-1));
				data [dwords-1] |= mask;

				mask = (uint)(0xFFFFFFFF >> (32 - remBits));
				data [dwords-1] &= mask;
			}

			else
				data [dwords-1] |= 0x80000000;

			Normalize ();
		}

		/// <summary>
		/// Randomizes the bits in "this" from the default RNG.
		/// </summary>
		public void Randomize ()
		{
			Randomize (Rng);
		}

		#endregion

		#region Bitwise

		public int BitCount ()
		{
			this.Normalize ();

			uint value = data [length - 1];
			uint mask = 0x80000000;
			uint bits = 32;

			while (bits > 0 && (value & mask) == 0) {
				bits--;
				mask >>= 1;
			}
			bits += ((length - 1) << 5);

			return (int)bits;
		}

		/// <summary>
		/// Tests if the specified bit is 1.
		/// </summary>
		/// <param name="bitNum">The bit to test. The least significant bit is 0.</param>
		/// <returns>True if bitNum is set to 1, else false.</returns>
#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public bool TestBit (uint bitNum)
		{
			uint bytePos = bitNum >> 5;             // divide by 32
			byte bitPos = (byte)(bitNum & 0x1F);    // get the lowest 5 bits

			uint mask = (uint)1 << bitPos;
			return ((this.data [bytePos] & mask) != 0);
		}

		public bool TestBit (int bitNum)
		{
			if (bitNum < 0) throw new IndexOutOfRangeException ("bitNum out of range");

			uint bytePos = (uint)bitNum >> 5;             // divide by 32
			byte bitPos = (byte)(bitNum & 0x1F);    // get the lowest 5 bits

			uint mask = (uint)1 << bitPos;
			return ((this.data [bytePos] | mask) == this.data [bytePos]);
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public void SetBit (uint bitNum)
		{
			SetBit (bitNum, true);
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public void ClearBit (uint bitNum)
		{
			SetBit (bitNum, false);
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public void SetBit (uint bitNum, bool value)
		{
			uint bytePos = bitNum >> 5;             // divide by 32

			if (bytePos < this.length) {
				uint mask = (uint)1 << (int)(bitNum & 0x1F);
				if (value)
					this.data [bytePos] |= mask;
				else
					this.data [bytePos] &= ~mask;
			}
		}

		public int LowestSetBit ()
		{
			if (this == 0) return -1;
			int i = 0;
			while (!TestBit (i)) i++;
			return i;
		}

		public byte[] GetBytes ()
		{
			if (this == 0) return new byte [1];

			int numBits = BitCount ();
			int numBytes = numBits >> 3;
			if ((numBits & 0x7) != 0)
				numBytes++;

			byte [] result = new byte [numBytes];

			int numBytesInWord = numBytes & 0x3;
			if (numBytesInWord == 0) numBytesInWord = 4;

			int pos = 0;
			for (int i = (int)length - 1; i >= 0; i--) {
				uint val = data [i];
				for (int j = numBytesInWord - 1; j >= 0; j--) {
					result [pos+j] = (byte)(val & 0xFF);
					val >>= 8;
				}
				pos += numBytesInWord;
				numBytesInWord = 4;
			}
			return result;
		}

		#endregion

		#region Compare

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public static bool operator == (BigInteger bi1, uint ui)
		{
			if (bi1.length != 1) bi1.Normalize ();
			return bi1.length == 1 && bi1.data [0] == ui;
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public static bool operator != (BigInteger bi1, uint ui)
		{
			if (bi1.length != 1) bi1.Normalize ();
			return !(bi1.length == 1 && bi1.data [0] == ui);
		}

		public static bool operator == (BigInteger bi1, BigInteger bi2)
		{
			// we need to compare with null
			if ((bi1 as object) == (bi2 as object))
				return true;
			if (null == bi1 || null == bi2)
				return false;
			return Kernel.Compare (bi1, bi2) == 0;
		}

		public static bool operator != (BigInteger bi1, BigInteger bi2)
		{
			// we need to compare with null
			if ((bi1 as object) == (bi2 as object))
				return false;
			if (null == bi1 || null == bi2)
				return true;
			return Kernel.Compare (bi1, bi2) != 0;
		}

		public static bool operator > (BigInteger bi1, BigInteger bi2)
		{
			return Kernel.Compare (bi1, bi2) > 0;
		}

		public static bool operator < (BigInteger bi1, BigInteger bi2)
		{
			return Kernel.Compare (bi1, bi2) < 0;
		}

		public static bool operator >= (BigInteger bi1, BigInteger bi2)
		{
			return Kernel.Compare (bi1, bi2) >= 0;
		}

		public static bool operator <= (BigInteger bi1, BigInteger bi2)
		{
			return Kernel.Compare (bi1, bi2) <= 0;
		}

		public Sign Compare (BigInteger bi)
		{
			return Kernel.Compare (this, bi);
		}

		#endregion

		#region Formatting

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public string ToString (uint radix)
		{
			return ToString (radix, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");
		}

#if !INSIDE_CORLIB
		[CLSCompliant (false)]
#endif 
		public string ToString (uint radix, string characterSet)
		{
			if (characterSet.Length < radix)
				throw new ArgumentException ("charSet length less than radix", "characterSet");
			if (radix == 1)
				throw new ArgumentException ("There is no such thing as radix one notation", "radix");

			if (this == 0) return "0";
			if (this == 1) return "1";

			string result = "";

			BigInteger a = new BigInteger (this);

			while (a != 0) {
				uint rem = Kernel.SingleByteDivideInPlace (a, radix);
				result = characterSet [(int) rem] + result;
			}

			return result;
		}

		#endregion

		#region Misc

		/// <summary>
		///     Normalizes this by setting the length to the actual number of
		///     uints used in data and by setting the sign to Sign.Zero if the
		///     value of this is 0.
		/// </summary>
		private void Normalize ()
		{
			// Normalize length
			while (length > 0 && data [length-1] == 0) length--;

			// Check for zero
			if (length == 0)
				length++;
		}

		public void Clear () 
		{
			for (int i=0; i < length; i++)
				data [i] = 0x00;
		}

		#endregion

		#region Object Impl

		public override int GetHashCode ()
		{
			uint val = 0;

			for (uint i = 0; i < this.length; i++)
				val ^= this.data [i];

			return (int)val;
		}

		public override string ToString ()
		{
			return ToString (10);
		}

		public override bool Equals (object o)
		{
			if (o == null)
				return false;
			if (o is int)
				return (int)o >= 0 && this == (uint)o;

			BigInteger bi = o as BigInteger;
			if (bi == null)
				return false;
			
			return Kernel.Compare (this, bi) == 0;
		}

		#endregion

		#region Number Theory

		public BigInteger GCD (BigInteger bi)
		{
			return Kernel.gcd (this, bi);
		}

		public BigInteger ModInverse (BigInteger modulus)
		{
			return Kernel.modInverse (this, modulus);
		}

		public BigInteger ModPow (BigInteger exp, BigInteger n)
		{
			ModulusRing mr = new ModulusRing (n);
			return mr.Pow (this, exp);
		}
		
		#endregion

		#region Prime Testing

		public bool IsProbablePrime ()
		{
			// can we use our small-prime table ?
			if (this <= smallPrimes[smallPrimes.Length - 1]) {
				for (int p = 0; p < smallPrimes.Length; p++) {
					if (this == smallPrimes[p])
						return true;
				}
				// the list is complete, so it's not a prime
				return false;
			}

			// otherwise check if we can divide by one of the small primes
			for (int p = 0; p < smallPrimes.Length; p++) {
				if (this % smallPrimes[p] == 0)
					return false;
			}
			// the last step is to confirm the "large" prime with the SPP or Miller-Rabin test
			return PrimalityTests.Test (this, Prime.ConfidenceFactor.Medium);
		}

		#endregion

		#region Prime Number Generation

		/// <summary>
		/// Generates the smallest prime >= bi
		/// </summary>
		/// <param name="bi">A BigInteger</param>
		/// <returns>The smallest prime >= bi. More mathematically, if bi is prime: bi, else Prime [PrimePi [bi] + 1].</returns>
		public static BigInteger NextHighestPrime (BigInteger bi)
		{
			NextPrimeFinder npf = new NextPrimeFinder ();
			return npf.GenerateNewPrime (0, bi);
		}

		public static BigInteger GeneratePseudoPrime (int bits)
		{
			SequentialSearchPrimeGeneratorBase sspg = new SequentialSearchPrimeGeneratorBase ();
			return sspg.GenerateNewPrime (bits);
		}

		/// <summary>
		/// Increments this by two
		/// </summary>
		public void Incr2 ()
		{
			int i = 0;

			data [0] += 2;

			// If there was no carry, nothing to do
			if (data [0] < 2) {

				// Account for the first carry
				data [++i]++;

				// Keep adding until no carry
				while (data [i++] == 0x0)
					data [i]++;

				// See if we increased the data length
				if (length == (uint)i)
					length++;
			}
		}

		#endregion

#if INSIDE_CORLIB
		internal
#else
		public
#endif
		sealed class ModulusRing {

			BigInteger mod, constant;

			public ModulusRing (BigInteger modulus)
			{
				this.mod = modulus;

				// calculate constant = b^ (2k) / m
				uint i = mod.length << 1;

				constant = new BigInteger (Sign.Positive, i + 1);
				constant.data [i] = 0x00000001;

				constant = constant / mod;
			}

			public void BarrettReduction (BigInteger x)
			{
				BigInteger n = mod;
				uint k = n.length,
					kPlusOne = k+1,
					kMinusOne = k-1;

				// x < mod, so nothing to do.
				if (x.length < k) return;

				BigInteger q3;

				//
				// Validate pointers
				//
				if (x.data.Length < x.length) throw new IndexOutOfRangeException ("x out of range");

				// q1 = x / b^ (k-1)
				// q2 = q1 * constant
				// q3 = q2 / b^ (k+1), Needs to be accessed with an offset of kPlusOne

				// TODO: We should the method in HAC p 604 to do this (14.45)
				q3 = new BigInteger (Sign.Positive, x.length - kMinusOne + constant.length);
				Kernel.Multiply (x.data, kMinusOne, x.length - kMinusOne, constant.data, 0, constant.length, q3.data, 0);

				// r1 = x mod b^ (k+1)
				// i.e. keep the lowest (k+1) words

				uint lengthToCopy = (x.length > kPlusOne) ? kPlusOne : x.length;

				x.length = lengthToCopy;
				x.Normalize ();

				// r2 = (q3 * n) mod b^ (k+1)
				// partial multiplication of q3 and n

				BigInteger r2 = new BigInteger (Sign.Positive, kPlusOne);
				Kernel.MultiplyMod2p32pmod (q3.data, (int)kPlusOne, (int)q3.length - (int)kPlusOne, n.data, 0, (int)n.length, r2.data, 0, (int)kPlusOne);

				r2.Normalize ();

				if (r2 <= x) {
					Kernel.MinusEq (x, r2);
				} else {
					BigInteger val = new BigInteger (Sign.Positive, kPlusOne + 1);
					val.data [kPlusOne] = 0x00000001;

					Kernel.MinusEq (val, r2);
					Kernel.PlusEq (x, val);
				}

				while (x >= n)
					Kernel.MinusEq (x, n);
			}

			public BigInteger Multiply (BigInteger a, BigInteger b)
			{
				if (a == 0 || b == 0) return 0;

				if (a > mod)
					a %= mod;

				if (b > mod)
					b %= mod;

				BigInteger ret = a * b;
				BarrettReduction (ret);

				return ret;
			}

			public BigInteger Difference (BigInteger a, BigInteger b)
			{
				Sign cmp = Kernel.Compare (a, b);
				BigInteger diff;

				switch (cmp) {
					case Sign.Zero:
						return 0;
					case Sign.Positive:
						diff = a - b; break;
					case Sign.Negative:
						diff = b - a; break;
					default:
						throw new Exception ();
				}

				if (diff >= mod) {
					if (diff.length >= mod.length << 1)
						diff %= mod;
					else
						BarrettReduction (diff);
				}
				if (cmp == Sign.Negative)
					diff = mod - diff;
				return diff;
			}
#if true
			public BigInteger Pow (BigInteger a, BigInteger k)
			{
				BigInteger b = new BigInteger (1);
				if (k == 0)
					return b;

				BigInteger A = a;
				if (k.TestBit (0))
					b = a;

				for (int i = 1; i < k.BitCount (); i++) {
					A = Multiply (A, A);
					if (k.TestBit (i))
						b = Multiply (A, b);
				}
				return b;
			}
#else
			public BigInteger Pow (BigInteger b, BigInteger exp)
			{
				if ((mod.data [0] & 1) == 1) return OddPow (b, exp);
				else return EvenPow (b, exp);
			}
			
			public BigInteger EvenPow (BigInteger b, BigInteger exp)
			{
				BigInteger resultNum = new BigInteger ((BigInteger)1, mod.length << 1);
				BigInteger tempNum = new BigInteger (b % mod, mod.length << 1);  // ensures (tempNum * tempNum) < b^ (2k)

				uint totalBits = (uint)exp.BitCount ();

				uint [] wkspace = new uint [mod.length << 1];

				// perform squaring and multiply exponentiation
				for (uint pos = 0; pos < totalBits; pos++) {
					if (exp.TestBit (pos)) {

						Array.Clear (wkspace, 0, wkspace.Length);
						Kernel.Multiply (resultNum.data, 0, resultNum.length, tempNum.data, 0, tempNum.length, wkspace, 0);
						resultNum.length += tempNum.length;
						uint [] t = wkspace;
						wkspace = resultNum.data;
						resultNum.data = t;

						BarrettReduction (resultNum);
					}

					Kernel.SquarePositive (tempNum, ref wkspace);
					BarrettReduction (tempNum);

					if (tempNum == 1) {
						return resultNum;
					}
				}

				return resultNum;
			}

			private BigInteger OddPow (BigInteger b, BigInteger exp)
			{
				BigInteger resultNum = new BigInteger (Montgomery.ToMont (1, mod), mod.length << 1);
				BigInteger tempNum = new BigInteger (Montgomery.ToMont (b, mod), mod.length << 1);  // ensures (tempNum * tempNum) < b^ (2k)
				uint mPrime = Montgomery.Inverse (mod.data [0]);
				uint totalBits = (uint)exp.BitCount ();

				uint [] wkspace = new uint [mod.length << 1];

				// perform squaring and multiply exponentiation
				for (uint pos = 0; pos < totalBits; pos++) {
					if (exp.TestBit (pos)) {

						Array.Clear (wkspace, 0, wkspace.Length);
						Kernel.Multiply (resultNum.data, 0, resultNum.length, tempNum.data, 0, tempNum.length, wkspace, 0);
						resultNum.length += tempNum.length;
						uint [] t = wkspace;
						wkspace = resultNum.data;
						resultNum.data = t;

						Montgomery.Reduce (resultNum, mod, mPrime);
					}

					// the value of tempNum is required in the last loop
					if (pos < totalBits - 1) {
						Kernel.SquarePositive (tempNum, ref wkspace);
						Montgomery.Reduce (tempNum, mod, mPrime);
					}
				}

				Montgomery.Reduce (resultNum, mod, mPrime);
				return resultNum;
			}
#endif
			#region Pow Small Base

			// TODO: Make tests for this, not really needed b/c prime stuff
			// checks it, but still would be nice
#if !INSIDE_CORLIB
                        [CLSCompliant (false)]
#endif 
#if true
			public BigInteger Pow (uint b, BigInteger exp)
			{
				return Pow (new BigInteger (b), exp);
			}
#else
			public BigInteger Pow (uint b, BigInteger exp)
			{
//				if (b != 2) {
					if ((mod.data [0] & 1) == 1)
						return OddPow (b, exp);
					else
						return EvenPow (b, exp);
/* buggy in some cases (like the well tested primes) 
				} else {
					if ((mod.data [0] & 1) == 1)
						return OddModTwoPow (exp);
					else 
						return EvenModTwoPow (exp);
				}*/
			}

			private unsafe BigInteger OddPow (uint b, BigInteger exp)
			{
				exp.Normalize ();
				uint [] wkspace = new uint [mod.length << 1 + 1];

				BigInteger resultNum = Montgomery.ToMont ((BigInteger)b, this.mod);
				resultNum = new BigInteger (resultNum, mod.length << 1 +1);

				uint mPrime = Montgomery.Inverse (mod.data [0]);

				int bc = exp.BitCount () - 2;
				uint pos = (bc > 1 ? (uint) bc : 1);

				//
				// We know that the first itr will make the val b
				//

				do {
					//
					// r = r ^ 2 % m
					//
					Kernel.SquarePositive (resultNum, ref wkspace);
					resultNum = Montgomery.Reduce (resultNum, mod, mPrime);

					if (exp.TestBit (pos)) {

						//
						// r = r * b % m
						//

						// TODO: Is Unsafe really speeding things up?
						fixed (uint* u = resultNum.data) {

							uint i = 0;
							ulong mc = 0;

							do {
								mc += (ulong)u [i] * (ulong)b;
								u [i] = (uint)mc;
								mc >>= 32;
							} while (++i < resultNum.length);

							if (resultNum.length < mod.length) {
								if (mc != 0) {
									u [i] = (uint)mc;
									resultNum.length++;
									while (resultNum >= mod)
										Kernel.MinusEq (resultNum, mod);
								}
							} else if (mc != 0) {

								//
								// First, we estimate the quotient by dividing
								// the first part of each of the numbers. Then
								// we correct this, if necessary, with a subtraction.
								//

								uint cc = (uint)mc;

								// We would rather have this estimate overshoot,
								// so we add one to the divisor
								uint divEstimate;
								if (mod.data [mod.length - 1] < UInt32.MaxValue) {
									divEstimate = (uint) ((((ulong)cc << 32) | (ulong) u [i -1]) /
										(mod.data [mod.length-1] + 1));
								}
								else {
									// guess but don't divide by 0
									divEstimate = (uint) ((((ulong)cc << 32) | (ulong) u [i -1]) /
										(mod.data [mod.length-1]));
								}

								uint t;

								i = 0;
								mc = 0;
								do {
									mc += (ulong)mod.data [i] * (ulong)divEstimate;
									t = u [i];
									u [i] -= (uint)mc;
									mc >>= 32;
									if (u [i] > t) mc++;
									i++;
								} while (i < resultNum.length);
								cc -= (uint)mc;

								if (cc != 0) {

									uint sc = 0, j = 0;
									uint [] s = mod.data;
									do {
										uint a = s [j];
										if (((a += sc) < sc) | ((u [j] -= a) > ~a)) sc = 1;
										else sc = 0;
										j++;
									} while (j < resultNum.length);
									cc -= sc;
								}
								while (resultNum >= mod)
									Kernel.MinusEq (resultNum, mod);
							} else {
								while (resultNum >= mod)
									Kernel.MinusEq (resultNum, mod);
							}
						}
					}
				} while (pos-- > 0);

				resultNum = Montgomery.Reduce (resultNum, mod, mPrime);
				return resultNum;

			}
			
			private unsafe BigInteger EvenPow (uint b, BigInteger exp)
			{
				exp.Normalize ();
				uint [] wkspace = new uint [mod.length << 1 + 1];
				BigInteger resultNum = new BigInteger ((BigInteger)b, mod.length << 1 + 1);

				uint pos = (uint)exp.BitCount () - 2;

				//
				// We know that the first itr will make the val b
				//

				do {
					//
					// r = r ^ 2 % m
					//
					Kernel.SquarePositive (resultNum, ref wkspace);
					if (!(resultNum.length < mod.length))
						BarrettReduction (resultNum);

					if (exp.TestBit (pos)) {

						//
						// r = r * b % m
						//

						// TODO: Is Unsafe really speeding things up?
						fixed (uint* u = resultNum.data) {

							uint i = 0;
							ulong mc = 0;

							do {
								mc += (ulong)u [i] * (ulong)b;
								u [i] = (uint)mc;
								mc >>= 32;
							} while (++i < resultNum.length);

							if (resultNum.length < mod.length) {
								if (mc != 0) {
									u [i] = (uint)mc;
									resultNum.length++;
									while (resultNum >= mod)
										Kernel.MinusEq (resultNum, mod);
								}
							} else if (mc != 0) {

								//
								// First, we estimate the quotient by dividing
								// the first part of each of the numbers. Then
								// we correct this, if necessary, with a subtraction.
								//

								uint cc = (uint)mc;

								// We would rather have this estimate overshoot,
								// so we add one to the divisor
								uint divEstimate = (uint) ((((ulong)cc << 32) | (ulong) u [i -1]) /
									(mod.data [mod.length-1] + 1));

								uint t;

								i = 0;
								mc = 0;
								do {
									mc += (ulong)mod.data [i] * (ulong)divEstimate;
									t = u [i];
									u [i] -= (uint)mc;
									mc >>= 32;
									if (u [i] > t) mc++;
									i++;
								} while (i < resultNum.length);
								cc -= (uint)mc;

								if (cc != 0) {

									uint sc = 0, j = 0;
									uint [] s = mod.data;
									do {
										uint a = s [j];
										if (((a += sc) < sc) | ((u [j] -= a) > ~a)) sc = 1;
										else sc = 0;
										j++;
									} while (j < resultNum.length);
									cc -= sc;
								}
								while (resultNum >= mod)
									Kernel.MinusEq (resultNum, mod);
							} else {
								while (resultNum >= mod)
									Kernel.MinusEq (resultNum, mod);
							}
						}
					}
				} while (pos-- > 0);

				return resultNum;
			}
#endif
/* known to be buggy in some cases */
#if false
			private unsafe BigInteger EvenModTwoPow (BigInteger exp)
			{
				exp.Normalize ();
				uint [] wkspace = new uint [mod.length << 1 + 1];

				BigInteger resultNum = new BigInteger (2, mod.length << 1 +1);

				uint value = exp.data [exp.length - 1];
				uint mask = 0x80000000;

				// Find the first bit of the exponent
				while ((value & mask) == 0)
					mask >>= 1;

				//
				// We know that the first itr will make the val 2,
				// so eat one bit of the exponent
				//
				mask >>= 1;

				uint wPos = exp.length - 1;

				do {
					value = exp.data [wPos];
					do {
						Kernel.SquarePositive (resultNum, ref wkspace);
						if (resultNum.length >= mod.length)
							BarrettReduction (resultNum);

						if ((value & mask) != 0) {
							//
							// resultNum = (resultNum * 2) % mod
							//

							fixed (uint* u = resultNum.data) {
								//
								// Double
								//
								uint* uu = u;
								uint* uuE = u + resultNum.length;
								uint x, carry = 0;
								while (uu < uuE) {
									x = *uu;
									*uu = (x << 1) | carry;
									carry = x >> (32 - 1);
									uu++;
								}

								// subtraction inlined because we know it is square
								if (carry != 0 || resultNum >= mod) {
									uu = u;
									uint c = 0;
									uint [] s = mod.data;
									uint i = 0;
									do {
										uint a = s [i];
										if (((a += c) < c) | ((* (uu++) -= a) > ~a))
											c = 1;
										else
											c = 0;
										i++;
									} while (uu < uuE);
								}
							}
						}
					} while ((mask >>= 1) > 0);
					mask = 0x80000000;
				} while (wPos-- > 0);

				return resultNum;
			}

			private unsafe BigInteger OddModTwoPow (BigInteger exp)
			{

				uint [] wkspace = new uint [mod.length << 1 + 1];

				BigInteger resultNum = Montgomery.ToMont ((BigInteger)2, this.mod);
				resultNum = new BigInteger (resultNum, mod.length << 1 +1);

				uint mPrime = Montgomery.Inverse (mod.data [0]);

				//
				// TODO: eat small bits, the ones we can do with no modular reduction
				//
				uint pos = (uint)exp.BitCount () - 2;

				do {
					Kernel.SquarePositive (resultNum, ref wkspace);
					resultNum = Montgomery.Reduce (resultNum, mod, mPrime);

					if (exp.TestBit (pos)) {
						//
						// resultNum = (resultNum * 2) % mod
						//

						fixed (uint* u = resultNum.data) {
							//
							// Double
							//
							uint* uu = u;
							uint* uuE = u + resultNum.length;
							uint x, carry = 0;
							while (uu < uuE) {
								x = *uu;
								*uu = (x << 1) | carry;
								carry = x >> (32 - 1);
								uu++;
							}

							// subtraction inlined because we know it is square
							if (carry != 0 || resultNum >= mod) {
								fixed (uint* s = mod.data) {
									uu = u;
									uint c = 0;
									uint* ss = s;
									do {
										uint a = *ss++;
										if (((a += c) < c) | ((* (uu++) -= a) > ~a))
											c = 1;
										else
											c = 0;
									} while (uu < uuE);
								}
							}
						}
					}
				} while (pos-- > 0);

				resultNum = Montgomery.Reduce (resultNum, mod, mPrime);
				return resultNum;
			}
#endif
			#endregion
		}

		/// <summary>
		/// Low level functions for the BigInteger
		/// </summary>
		private sealed class Kernel {

			#region Addition/Subtraction

			/// <summary>
			/// Adds two numbers with the same sign.
			/// </summary>
			/// <param name="bi1">A BigInteger</param>
			/// <param name="bi2">A BigInteger</param>
			/// <returns>bi1 + bi2</returns>
			public static BigInteger AddSameSign (BigInteger bi1, BigInteger bi2)
			{
				uint [] x, y;
				uint yMax, xMax, i = 0;

				// x should be bigger
				if (bi1.length < bi2.length) {
					x = bi2.data;
					xMax = bi2.length;
					y = bi1.data;
					yMax = bi1.length;
				} else {
					x = bi1.data;
					xMax = bi1.length;
					y = bi2.data;
					yMax = bi2.length;
				}
				
				BigInteger result = new BigInteger (Sign.Positive, xMax + 1);

				uint [] r = result.data;

				ulong sum = 0;

				// Add common parts of both numbers
				do {
					sum = ((ulong)x [i]) + ((ulong)y [i]) + sum;
					r [i] = (uint)sum;
					sum >>= 32;
				} while (++i < yMax);

				// Copy remainder of longer number while carry propagation is required
				bool carry = (sum != 0);

				if (carry) {

					if (i < xMax) {
						do
							carry = ((r [i] = x [i] + 1) == 0);
						while (++i < xMax && carry);
					}

					if (carry) {
						r [i] = 1;
						result.length = ++i;
						return result;
					}
				}

				// Copy the rest
				if (i < xMax) {
					do
						r [i] = x [i];
					while (++i < xMax);
				}

				result.Normalize ();
				return result;
			}

			public static BigInteger Subtract (BigInteger big, BigInteger small)
			{
				BigInteger result = new BigInteger (Sign.Positive, big.length);

				uint [] r = result.data, b = big.data, s = small.data;
				uint i = 0, c = 0;

				do {

					uint x = s [i];
					if (((x += c) < c) | ((r [i] = b [i] - x) > ~x))
						c = 1;
					else
						c = 0;

				} while (++i < small.length);

				if (i == big.length) goto fixup;

				if (c == 1) {
					do
						r [i] = b [i] - 1;
					while (b [i++] == 0 && i < big.length);

					if (i == big.length) goto fixup;
				}

				do
					r [i] = b [i];
				while (++i < big.length);

				fixup:

					result.Normalize ();
				return result;
			}

			public static void MinusEq (BigInteger big, BigInteger small)
			{
				uint [] b = big.data, s = small.data;
				uint i = 0, c = 0;

				do {
					uint x = s [i];
					if (((x += c) < c) | ((b [i] -= x) > ~x))
						c = 1;
					else
						c = 0;
				} while (++i < small.length);

				if (i == big.length) goto fixup;

				if (c == 1) {
					do
						b [i]--;
					while (b [i++] == 0 && i < big.length);
				}

				fixup:

					// Normalize length
					while (big.length > 0 && big.data [big.length-1] == 0) big.length--;

				// Check for zero
				if (big.length == 0)
					big.length++;

			}

			public static void PlusEq (BigInteger bi1, BigInteger bi2)
			{
				uint [] x, y;
				uint yMax, xMax, i = 0;
				bool flag = false;

				// x should be bigger
				if (bi1.length < bi2.length){
					flag = true;
					x = bi2.data;
					xMax = bi2.length;
					y = bi1.data;
					yMax = bi1.length;
				} else {
					x = bi1.data;
					xMax = bi1.length;
					y = bi2.data;
					yMax = bi2.length;
				}

				uint [] r = bi1.data;

				ulong sum = 0;

				// Add common parts of both numbers
				do {
					sum += ((ulong)x [i]) + ((ulong)y [i]);
					r [i] = (uint)sum;
					sum >>= 32;
				} while (++i < yMax);

				// Copy remainder of longer number while carry propagation is required
				bool carry = (sum != 0);

				if (carry){

					if (i < xMax) {
						do
							carry = ((r [i] = x [i] + 1) == 0);
						while (++i < xMax && carry);
					}

					if (carry) {
						r [i] = 1;
						bi1.length = ++i;
						return;
					}
				}

				// Copy the rest
				if (flag && i < xMax - 1) {
					do
						r [i] = x [i];
					while (++i < xMax);
				}

				bi1.length = xMax + 1;
				bi1.Normalize ();
			}

			#endregion

			#region Compare

			/// <summary>
			/// Compares two BigInteger
			/// </summary>
			/// <param name="bi1">A BigInteger</param>
			/// <param name="bi2">A BigInteger</param>
			/// <returns>The sign of bi1 - bi2</returns>
			public static Sign Compare (BigInteger bi1, BigInteger bi2)
			{
				//
				// Step 1. Compare the lengths
				//
				uint l1 = bi1.length, l2 = bi2.length;

				while (l1 > 0 && bi1.data [l1-1] == 0) l1--;
				while (l2 > 0 && bi2.data [l2-1] == 0) l2--;

				if (l1 == 0 && l2 == 0) return Sign.Zero;

				// bi1 len < bi2 len
				if (l1 < l2) return Sign.Negative;
				// bi1 len > bi2 len
				else if (l1 > l2) return Sign.Positive;

				//
				// Step 2. Compare the bits
				//

				uint pos = l1 - 1;

				while (pos != 0 && bi1.data [pos] == bi2.data [pos]) pos--;
				
				if (bi1.data [pos] < bi2.data [pos])
					return Sign.Negative;
				else if (bi1.data [pos] > bi2.data [pos])
					return Sign.Positive;
				else
					return Sign.Zero;
			}

			#endregion

			#region Division

			#region Dword

			/// <summary>
			/// Performs n / d and n % d in one operation.
			/// </summary>
			/// <param name="n">A BigInteger, upon exit this will hold n / d</param>
			/// <param name="d">The divisor</param>
			/// <returns>n % d</returns>
			public static uint SingleByteDivideInPlace (BigInteger n, uint d)
			{
				ulong r = 0;
				uint i = n.length;

				while (i-- > 0) {
					r <<= 32;
					r |= n.data [i];
					n.data [i] = (uint)(r / d);
					r %= d;
				}
				n.Normalize ();

				return (uint)r;
			}

			public static uint DwordMod (BigInteger n, uint d)
			{
				ulong r = 0;
				uint i = n.length;

				while (i-- > 0) {
					r <<= 32;
					r |= n.data [i];
					r %= d;
				}

				return (uint)r;
			}

			public static BigInteger DwordDiv (BigInteger n, uint d)
			{
				BigInteger ret = new BigInteger (Sign.Positive, n.length);

				ulong r = 0;
				uint i = n.length;

				while (i-- > 0) {
					r <<= 32;
					r |= n.data [i];
					ret.data [i] = (uint)(r / d);
					r %= d;
				}
				ret.Normalize ();

				return ret;
			}

			public static BigInteger [] DwordDivMod (BigInteger n, uint d)
			{
				BigInteger ret = new BigInteger (Sign.Positive , n.length);

				ulong r = 0;
				uint i = n.length;

				while (i-- > 0) {
					r <<= 32;
					r |= n.data [i];
					ret.data [i] = (uint)(r / d);
					r %= d;
				}
				ret.Normalize ();

				BigInteger rem = (uint)r;

				return new BigInteger [] {ret, rem};
			}

				#endregion

			#region BigNum

			public static BigInteger [] multiByteDivide (BigInteger bi1, BigInteger bi2)
			{
				if (Kernel.Compare (bi1, bi2) == Sign.Negative)
					return new BigInteger [2] { 0, new BigInteger (bi1) };

				bi1.Normalize (); bi2.Normalize ();

				if (bi2.length == 1)
					return DwordDivMod (bi1, bi2.data [0]);

				uint remainderLen = bi1.length + 1;
				int divisorLen = (int)bi2.length + 1;

				uint mask = 0x80000000;
				uint val = bi2.data [bi2.length - 1];
				int shift = 0;
				int resultPos = (int)bi1.length - (int)bi2.length;

				while (mask != 0 && (val & mask) == 0) {
					shift++; mask >>= 1;
				}

				BigInteger quot = new BigInteger (Sign.Positive, bi1.length - bi2.length + 1);
				BigInteger rem = (bi1 << shift);

				uint [] remainder = rem.data;

				bi2 = bi2 << shift;

				int j = (int)(remainderLen - bi2.length);
				int pos = (int)remainderLen - 1;

				uint firstDivisorByte = bi2.data [bi2.length-1];
				ulong secondDivisorByte = bi2.data [bi2.length-2];

				while (j > 0) {
					ulong dividend = ((ulong)remainder [pos] << 32) + (ulong)remainder [pos-1];

					ulong q_hat = dividend / (ulong)firstDivisorByte;
					ulong r_hat = dividend % (ulong)firstDivisorByte;

					do {

						if (q_hat == 0x100000000 ||
							(q_hat * secondDivisorByte) > ((r_hat << 32) + remainder [pos-2])) {
							q_hat--;
							r_hat += (ulong)firstDivisorByte;

							if (r_hat < 0x100000000)
								continue;
						}
						break;
					} while (true);

					//
					// At this point, q_hat is either exact, or one too large
					// (more likely to be exact) so, we attempt to multiply the
					// divisor by q_hat, if we get a borrow, we just subtract
					// one from q_hat and add the divisor back.
					//

					uint t;
					uint dPos = 0;
					int nPos = pos - divisorLen + 1;
					ulong mc = 0;
					uint uint_q_hat = (uint)q_hat;
					do {
						mc += (ulong)bi2.data [dPos] * (ulong)uint_q_hat;
						t = remainder [nPos];
						remainder [nPos] -= (uint)mc;
						mc >>= 32;
						if (remainder [nPos] > t) mc++;
						dPos++; nPos++;
					} while (dPos < divisorLen);

					nPos = pos - divisorLen + 1;
					dPos = 0;

					// Overestimate
					if (mc != 0) {
						uint_q_hat--;
						ulong sum = 0;

						do {
							sum = ((ulong)remainder [nPos]) + ((ulong)bi2.data [dPos]) + sum;
							remainder [nPos] = (uint)sum;
							sum >>= 32;
							dPos++; nPos++;
						} while (dPos < divisorLen);

					}

					quot.data [resultPos--] = (uint)uint_q_hat;

					pos--;
					j--;
				}

				quot.Normalize ();
				rem.Normalize ();
				BigInteger [] ret = new BigInteger [2] { quot, rem };

				if (shift != 0)
					ret [1] >>= shift;

				return ret;
			}

			#endregion

			#endregion

			#region Shift
			public static BigInteger LeftShift (BigInteger bi, int n)
			{
				if (n == 0) return new BigInteger (bi, bi.length + 1);

				int w = n >> 5;
				n &= ((1 << 5) - 1);

				BigInteger ret = new BigInteger (Sign.Positive, bi.length + 1 + (uint)w);

				uint i = 0, l = bi.length;
				if (n != 0) {
					uint x, carry = 0;
					while (i < l) {
						x = bi.data [i];
						ret.data [i + w] = (x << n) | carry;
						carry = x >> (32 - n);
						i++;
					}
					ret.data [i + w] = carry;
				} else {
					while (i < l) {
						ret.data [i + w] = bi.data [i];
						i++;
					}
				}

				ret.Normalize ();
				return ret;
			}

			public static BigInteger RightShift (BigInteger bi, int n)
			{
				if (n == 0) return new BigInteger (bi);

				int w = n >> 5;
				int s = n & ((1 << 5) - 1);

				BigInteger ret = new BigInteger (Sign.Positive, bi.length - (uint)w + 1);
				uint l = (uint)ret.data.Length - 1;

				if (s != 0) {

					uint x, carry = 0;

					while (l-- > 0) {
						x = bi.data [l + w];
						ret.data [l] = (x >> n) | carry;
						carry = x << (32 - n);
					}
				} else {
					while (l-- > 0)
						ret.data [l] = bi.data [l + w];

				}
				ret.Normalize ();
				return ret;
			}

			#endregion

			#region Multiply

			public static BigInteger MultiplyByDword (BigInteger n, uint f)
			{
				BigInteger ret = new BigInteger (Sign.Positive, n.length + 1);

				uint i = 0;
				ulong c = 0;

				do {
					c += (ulong)n.data [i] * (ulong)f;
					ret.data [i] = (uint)c;
					c >>= 32;
				} while (++i < n.length);
				ret.data [i] = (uint)c;
				ret.Normalize ();
				return ret;

			}

			/// <summary>
			/// Multiplies the data in x [xOffset:xOffset+xLen] by
			/// y [yOffset:yOffset+yLen] and puts it into
			/// d [dOffset:dOffset+xLen+yLen].
			/// </summary>
			/// <remarks>
			/// This code is unsafe! It is the caller's responsibility to make
			/// sure that it is safe to access x [xOffset:xOffset+xLen],
			/// y [yOffset:yOffset+yLen], and d [dOffset:dOffset+xLen+yLen].
			/// </remarks>
			public static unsafe void Multiply (uint [] x, uint xOffset, uint xLen, uint [] y, uint yOffset, uint yLen, uint [] d, uint dOffset)
			{
				fixed (uint* xx = x, yy = y, dd = d) {
					uint* xP = xx + xOffset,
						xE = xP + xLen,
						yB = yy + yOffset,
						yE = yB + yLen,
						dB = dd + dOffset;

					for (; xP < xE; xP++, dB++) {

						if (*xP == 0) continue;

						ulong mcarry = 0;

						uint* dP = dB;
						for (uint* yP = yB; yP < yE; yP++, dP++) {
							mcarry += ((ulong)*xP * (ulong)*yP) + (ulong)*dP;

							*dP = (uint)mcarry;
							mcarry >>= 32;
						}

						if (mcarry != 0)
							*dP = (uint)mcarry;
					}
				}
			}

			/// <summary>
			/// Multiplies the data in x [xOffset:xOffset+xLen] by
			/// y [yOffset:yOffset+yLen] and puts the low mod words into
			/// d [dOffset:dOffset+mod].
			/// </summary>
			/// <remarks>
			/// This code is unsafe! It is the caller's responsibility to make
			/// sure that it is safe to access x [xOffset:xOffset+xLen],
			/// y [yOffset:yOffset+yLen], and d [dOffset:dOffset+mod].
			/// </remarks>
			public static unsafe void MultiplyMod2p32pmod (uint [] x, int xOffset, int xLen, uint [] y, int yOffest, int yLen, uint [] d, int dOffset, int mod)
			{
				fixed (uint* xx = x, yy = y, dd = d) {
					uint* xP = xx + xOffset,
						xE = xP + xLen,
						yB = yy + yOffest,
						yE = yB + yLen,
						dB = dd + dOffset,
						dE = dB + mod;

					for (; xP < xE; xP++, dB++) {

						if (*xP == 0) continue;

						ulong mcarry = 0;
						uint* dP = dB;
						for (uint* yP = yB; yP < yE && dP < dE; yP++, dP++) {
							mcarry += ((ulong)*xP * (ulong)*yP) + (ulong)*dP;

							*dP = (uint)mcarry;
							mcarry >>= 32;
						}

						if (mcarry != 0 && dP < dE)
							*dP = (uint)mcarry;
					}
				}
			}

			public static unsafe void SquarePositive (BigInteger bi, ref uint [] wkSpace)
			{
				uint [] t = wkSpace;
				wkSpace = bi.data;
				uint [] d = bi.data;
				uint dl = bi.length;
				bi.data = t;

				fixed (uint* dd = d, tt = t) {

					uint* ttE = tt + t.Length;
					// Clear the dest
					for (uint* ttt = tt; ttt < ttE; ttt++)
						*ttt = 0;

					uint* dP = dd, tP = tt;

					for (uint i = 0; i < dl; i++, dP++) {
						if (*dP == 0)
							continue;

						ulong mcarry = 0;
						uint bi1val = *dP;

						uint* dP2 = dP + 1, tP2 = tP + 2*i + 1;

						for (uint j = i + 1; j < dl; j++, tP2++, dP2++) {
							// k = i + j
							mcarry += ((ulong)bi1val * (ulong)*dP2) + *tP2;

							*tP2 = (uint)mcarry;
							mcarry >>= 32;
						}

						if (mcarry != 0)
							*tP2 = (uint)mcarry;
					}

					// Double t. Inlined for speed.

					tP = tt;

					uint x, carry = 0;
					while (tP < ttE) {
						x = *tP;
						*tP = (x << 1) | carry;
						carry = x >> (32 - 1);
						tP++;
					}
					if (carry != 0) *tP = carry;

					// Add in the diagnals

					dP = dd;
					tP = tt;
					for (uint* dE = dP + dl; (dP < dE); dP++, tP++) {
						ulong val = (ulong)*dP * (ulong)*dP + *tP;
						*tP = (uint)val;
						val >>= 32;
						*(++tP) += (uint)val;
						if (*tP < (uint)val) {
							uint* tP3 = tP;
							// Account for the first carry
							(*++tP3)++;

							// Keep adding until no carry
							while ((*tP3++) == 0)
								(*tP3)++;
						}

					}

					bi.length <<= 1;

					// Normalize length
					while (tt [bi.length-1] == 0 && bi.length > 1) bi.length--;

				}
			}

/* 
 * Never called in BigInteger (and part of a private class)
 * 			public static bool Double (uint [] u, int l)
			{
				uint x, carry = 0;
				uint i = 0;
				while (i < l) {
					x = u [i];
					u [i] = (x << 1) | carry;
					carry = x >> (32 - 1);
					i++;
				}
				if (carry != 0) u [l] = carry;
				return carry != 0;
			}*/

			#endregion

			#region Number Theory

			public static BigInteger gcd (BigInteger a, BigInteger b)
			{
				BigInteger x = a;
				BigInteger y = b;

				BigInteger g = y;

				while (x.length > 1) {
					g = x;
					x = y % x;
					y = g;

				}
				if (x == 0) return g;

				// TODO: should we have something here if we can convert to long?

				//
				// Now we can just do it with single precision. I am using the binary gcd method,
				// as it should be faster.
				//

				uint yy = x.data [0];
				uint xx = y % yy;

				int t = 0;

				while (((xx | yy) & 1) == 0) {
					xx >>= 1; yy >>= 1; t++;
				}
				while (xx != 0) {
					while ((xx & 1) == 0) xx >>= 1;
					while ((yy & 1) == 0) yy >>= 1;
					if (xx >= yy)
						xx = (xx - yy) >> 1;
					else
						yy = (yy - xx) >> 1;
				}

				return yy << t;
			}

			public static uint modInverse (BigInteger bi, uint modulus)
			{
				uint a = modulus, b = bi % modulus;
				uint p0 = 0, p1 = 1;

				while (b != 0) {
					if (b == 1)
						return p1;
					p0 += (a / b) * p1;
					a %= b;

					if (a == 0)
						break;
					if (a == 1)
						return modulus-p0;

					p1 += (b / a) * p0;
					b %= a;

				}
				return 0;
			}
			
			public static BigInteger modInverse (BigInteger bi, BigInteger modulus)
			{
				if (modulus.length == 1) return modInverse (bi, modulus.data [0]);

				BigInteger [] p = { 0, 1 };
				BigInteger [] q = new BigInteger [2];    // quotients
				BigInteger [] r = { 0, 0 };             // remainders

				int step = 0;

				BigInteger a = modulus;
				BigInteger b = bi;

				ModulusRing mr = new ModulusRing (modulus);

				while (b != 0) {

					if (step > 1) {

						BigInteger pval = mr.Difference (p [0], p [1] * q [0]);
						p [0] = p [1]; p [1] = pval;
					}

					BigInteger [] divret = multiByteDivide (a, b);

					q [0] = q [1]; q [1] = divret [0];
					r [0] = r [1]; r [1] = divret [1];
					a = b;
					b = divret [1];

					step++;
				}

				if (r [0] != 1)
					throw (new ArithmeticException ("No inverse!"));

				return mr.Difference (p [0], p [1] * q [0]);

			}
			#endregion
		}
	}
}
