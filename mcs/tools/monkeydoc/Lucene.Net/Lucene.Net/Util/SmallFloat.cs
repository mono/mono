/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Mono.Lucene.Net.Util
{
	
	
	/// <summary>Floating point numbers smaller than 32 bits.
	/// 
	/// </summary>
	/// <version>  $Id$
	/// </version>
	public class SmallFloat
	{
		
		/// <summary>Converts a 32 bit float to an 8 bit float.
		/// <br/>Values less than zero are all mapped to zero.
		/// <br/>Values are truncated (rounded down) to the nearest 8 bit value.
		/// <br/>Values between zero and the smallest representable value
		/// are rounded up.
		/// 
		/// </summary>
		/// <param name="f">the 32 bit float to be converted to an 8 bit float (byte)
		/// </param>
		/// <param name="numMantissaBits">the number of mantissa bits to use in the byte, with the remainder to be used in the exponent
		/// </param>
		/// <param name="zeroExp">the zero-point in the range of exponent values
		/// </param>
		/// <returns> the 8 bit float representation
		/// </returns>
		public static sbyte FloatToByte(float f, int numMantissaBits, int zeroExp)
		{
			// Adjustment from a float zero exponent to our zero exponent,
			// shifted over to our exponent position.
			int fzero = (63 - zeroExp) << numMantissaBits;
			int bits = System.BitConverter.ToInt32(System.BitConverter.GetBytes(f), 0);
			int smallfloat = bits >> (24 - numMantissaBits);
			if (smallfloat < fzero)
			{
				return (bits <= 0)?(sbyte) 0:(sbyte) 1; // underflow is mapped to smallest non-zero number.
			}
			else if (smallfloat >= fzero + 0x100)
			{
				return - 1; // overflow maps to largest number
			}
			else
			{
				return (sbyte) (smallfloat - fzero);
			}
		}
		
		/// <summary>Converts an 8 bit float to a 32 bit float. </summary>
		public static float ByteToFloat(byte b, int numMantissaBits, int zeroExp)
		{
			// on Java1.5 & 1.6 JVMs, prebuilding a decoding array and doing a lookup
			// is only a little bit faster (anywhere from 0% to 7%)
			if (b == 0)
				return 0.0f;
			int bits = (b & 0xff) << (24 - numMantissaBits);
			bits += ((63 - zeroExp) << 24);
			return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
		}
		
		
		//
		// Some specializations of the generic functions follow.
		// The generic functions are just as fast with current (1.5)
		// -server JVMs, but still slower with client JVMs.
		//
		
		/// <summary>floatToByte(b, mantissaBits=3, zeroExponent=15)
		/// <br/>smallest non-zero value = 5.820766E-10
		/// <br/>largest value = 7.5161928E9
		/// <br/>epsilon = 0.125
		/// </summary>
		public static sbyte FloatToByte315(float f)
		{
			int bits = System.BitConverter.ToInt32(System.BitConverter.GetBytes(f), 0);
			int smallfloat = bits >> (24 - 3);
			if (smallfloat < (63 - 15) << 3)
			{
				return (bits <= 0)?(sbyte) 0:(sbyte) 1;
			}
			if (smallfloat >= ((63 - 15) << 3) + 0x100)
			{
				return - 1;
			}
			return (sbyte) (smallfloat - ((63 - 15) << 3));
		}
		
		/// <summary>byteToFloat(b, mantissaBits=3, zeroExponent=15) </summary>
		public static float Byte315ToFloat(byte b)
		{
			// on Java1.5 & 1.6 JVMs, prebuilding a decoding array and doing a lookup
			// is only a little bit faster (anywhere from 0% to 7%)
			if (b == 0)
				return 0.0f;
			int bits = (b & 0xff) << (24 - 3);
			bits += ((63 - 15) << 24);
			return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
		}
		
		
		/// <summary>floatToByte(b, mantissaBits=5, zeroExponent=2)
		/// <br/>smallest nonzero value = 0.033203125
		/// <br/>largest value = 1984.0
		/// <br/>epsilon = 0.03125
		/// </summary>
		public static sbyte FloatToByte52(float f)
		{
			int bits = System.BitConverter.ToInt32(System.BitConverter.GetBytes(f), 0);
			int smallfloat = bits >> (24 - 5);
			if (smallfloat < (63 - 2) << 5)
			{
				return (bits <= 0)?(sbyte) 0:(sbyte) 1;
			}
			if (smallfloat >= ((63 - 2) << 5) + 0x100)
			{
				return - 1;
			}
			return (sbyte) (smallfloat - ((63 - 2) << 5));
		}
		
		/// <summary>byteToFloat(b, mantissaBits=5, zeroExponent=2) </summary>
		public static float Byte52ToFloat(byte b)
		{
			// on Java1.5 & 1.6 JVMs, prebuilding a decoding array and doing a lookup
			// is only a little bit faster (anywhere from 0% to 7%)
			if (b == 0)
				return 0.0f;
			int bits = (b & 0xff) << (24 - 5);
			bits += ((63 - 2) << 24);
			return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
		}
	}
}
