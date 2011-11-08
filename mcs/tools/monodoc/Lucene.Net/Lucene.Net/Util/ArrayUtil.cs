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
	
	/// <summary> Methods for manipulating arrays.</summary>
	public sealed class ArrayUtil
	{
		/*
		Begin Apache Harmony code
		
		Revision taken on Friday, June 12. https://svn.apache.org/repos/asf/harmony/enhanced/classlib/archive/java6/modules/luni/src/main/java/java/lang/Integer.java
		
		*/
		
		/// <summary> Parses the string argument as if it was an int value and returns the
		/// result. Throws NumberFormatException if the string does not represent an
		/// int quantity.
		/// 
		/// </summary>
		/// <param name="chars">a string representation of an int quantity.
		/// </param>
		/// <returns> int the value represented by the argument
		/// </returns>
		/// <throws>  NumberFormatException if the argument could not be parsed as an int quantity. </throws>
		public static int ParseInt(char[] chars)
		{
			return ParseInt(chars, 0, chars.Length, 10);
		}
		
		/// <summary> Parses a char array into an int.</summary>
		/// <param name="chars">the character array
		/// </param>
		/// <param name="offset">The offset into the array
		/// </param>
		/// <param name="len">The length
		/// </param>
		/// <returns> the int
		/// </returns>
		/// <throws>  NumberFormatException if it can't parse </throws>
		public static int ParseInt(char[] chars, int offset, int len)
		{
			return ParseInt(chars, offset, len, 10);
		}
		
		/// <summary> Parses the string argument as if it was an int value and returns the
		/// result. Throws NumberFormatException if the string does not represent an
		/// int quantity. The second argument specifies the radix to use when parsing
		/// the value.
		/// 
		/// </summary>
		/// <param name="chars">a string representation of an int quantity.
		/// </param>
		/// <param name="radix">the base to use for conversion.
		/// </param>
		/// <returns> int the value represented by the argument
		/// </returns>
		/// <throws>  NumberFormatException if the argument could not be parsed as an int quantity. </throws>
		public static int ParseInt(char[] chars, int offset, int len, int radix)
		{
			if (chars == null || radix < 2 || radix > 36)
			{
				throw new System.FormatException();
			}
			int i = 0;
			if (len == 0)
			{
				throw new System.FormatException("chars length is 0");
			}
			bool negative = chars[offset + i] == '-';
			if (negative && ++i == len)
			{
				throw new System.FormatException("can't convert to an int");
			}
			if (negative == true)
			{
				offset++;
				len--;
			}
			return Parse(chars, offset, len, radix, negative);
		}
		
		
		private static int Parse(char[] chars, int offset, int len, int radix, bool negative)
		{
			int max = System.Int32.MinValue / radix;
			int result = 0;
			for (int i = 0; i < len; i++)
			{
				int digit = (int) System.Char.GetNumericValue(chars[i + offset]);
				if (digit == - 1)
				{
					throw new System.FormatException("Unable to parse");
				}
				if (max > result)
				{
					throw new System.FormatException("Unable to parse");
				}
				int next = result * radix - digit;
				if (next > result)
				{
					throw new System.FormatException("Unable to parse");
				}
				result = next;
			}
			/*while (offset < len) {
			
			}*/
			if (!negative)
			{
				result = - result;
				if (result < 0)
				{
					throw new System.FormatException("Unable to parse");
				}
			}
			return result;
		}
		
		
		/*
		
		END APACHE HARMONY CODE
		*/
		
		
		public static int GetNextSize(int targetSize)
		{
			/* This over-allocates proportional to the list size, making room
			* for additional growth.  The over-allocation is mild, but is
			* enough to give linear-time amortized behavior over a long
			* sequence of appends() in the presence of a poorly-performing
			* system realloc().
			* The growth pattern is:  0, 4, 8, 16, 25, 35, 46, 58, 72, 88, ...
			*/
			return (targetSize >> 3) + (targetSize < 9?3:6) + targetSize;
		}
		
		public static int GetShrinkSize(int currentSize, int targetSize)
		{
			int newSize = GetNextSize(targetSize);
			// Only reallocate if we are "substantially" smaller.
			// This saves us from "running hot" (constantly making a
			// bit bigger then a bit smaller, over and over):
			if (newSize < currentSize / 2)
				return newSize;
			else
				return currentSize;
		}
		
		public static int[] Grow(int[] array, int minSize)
		{
			if (array.Length < minSize)
			{
				int[] newArray = new int[GetNextSize(minSize)];
				Array.Copy(array, 0, newArray, 0, array.Length);
				return newArray;
			}
			else
				return array;
		}
		
		public static int[] Grow(int[] array)
		{
			return Grow(array, 1 + array.Length);
		}
		
		public static int[] Shrink(int[] array, int targetSize)
		{
			int newSize = GetShrinkSize(array.Length, targetSize);
			if (newSize != array.Length)
			{
				int[] newArray = new int[newSize];
				Array.Copy(array, 0, newArray, 0, newSize);
				return newArray;
			}
			else
				return array;
		}
		
		public static long[] Grow(long[] array, int minSize)
		{
			if (array.Length < minSize)
			{
				long[] newArray = new long[GetNextSize(minSize)];
				Array.Copy(array, 0, newArray, 0, array.Length);
				return newArray;
			}
			else
				return array;
		}
		
		public static long[] Grow(long[] array)
		{
			return Grow(array, 1 + array.Length);
		}
		
		public static long[] Shrink(long[] array, int targetSize)
		{
			int newSize = GetShrinkSize(array.Length, targetSize);
			if (newSize != array.Length)
			{
				long[] newArray = new long[newSize];
				Array.Copy(array, 0, newArray, 0, newSize);
				return newArray;
			}
			else
				return array;
		}
		
		public static byte[] Grow(byte[] array, int minSize)
		{
			if (array.Length < minSize)
			{
				byte[] newArray = new byte[GetNextSize(minSize)];
				Array.Copy(array, 0, newArray, 0, array.Length);
				return newArray;
			}
			else
				return array;
		}
		
		public static byte[] Grow(byte[] array)
		{
			return Grow(array, 1 + array.Length);
		}
		
		public static byte[] Shrink(byte[] array, int targetSize)
		{
			int newSize = GetShrinkSize(array.Length, targetSize);
			if (newSize != array.Length)
			{
				byte[] newArray = new byte[newSize];
				Array.Copy(array, 0, newArray, 0, newSize);
				return newArray;
			}
			else
				return array;
		}
		
		/// <summary> Returns hash of chars in range start (inclusive) to
		/// end (inclusive)
		/// </summary>
		public static int HashCode(char[] array, int start, int end)
		{
			int code = 0;
			for (int i = end - 1; i >= start; i--)
				code = code * 31 + array[i];
			return code;
		}
		
		/// <summary> Returns hash of chars in range start (inclusive) to
		/// end (inclusive)
		/// </summary>
		public static int HashCode(byte[] array, int start, int end)
		{
			int code = 0;
			for (int i = end - 1; i >= start; i--)
				code = code * 31 + array[i];
			return code;
		}
	}
}
