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

using NumericTokenStream = Mono.Lucene.Net.Analysis.NumericTokenStream;
using NumericField = Mono.Lucene.Net.Documents.NumericField;
using NumericRangeFilter = Mono.Lucene.Net.Search.NumericRangeFilter;
using NumericRangeQuery = Mono.Lucene.Net.Search.NumericRangeQuery;

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> This is a helper class to generate prefix-encoded representations for numerical values
	/// and supplies converters to represent float/double values as sortable integers/longs.
	/// 
	/// <p/>To quickly execute range queries in Apache Lucene, a range is divided recursively
	/// into multiple intervals for searching: The center of the range is searched only with
	/// the lowest possible precision in the trie, while the boundaries are matched
	/// more exactly. This reduces the number of terms dramatically.
	/// 
	/// <p/>This class generates terms to achive this: First the numerical integer values need to
	/// be converted to strings. For that integer values (32 bit or 64 bit) are made unsigned
	/// and the bits are converted to ASCII chars with each 7 bit. The resulting string is
	/// sortable like the original integer value. Each value is also prefixed
	/// (in the first char) by the <code>shift</code> value (number of bits removed) used
	/// during encoding.
	/// 
	/// <p/>To also index floating point numbers, this class supplies two methods to convert them
	/// to integer values by changing their bit layout: {@link #doubleToSortableLong},
	/// {@link #floatToSortableInt}. You will have no precision loss by
	/// converting floating point numbers to integers and back (only that the integer form
	/// is not usable). Other data types like dates can easily converted to longs or ints (e.g.
	/// date to long: {@link java.util.Date#getTime}).
	/// 
	/// <p/>For easy usage, the trie algorithm is implemented for indexing inside
	/// {@link NumericTokenStream} that can index <code>int</code>, <code>long</code>,
	/// <code>float</code>, and <code>double</code>. For querying,
	/// {@link NumericRangeQuery} and {@link NumericRangeFilter} implement the query part
	/// for the same data types.
	/// 
	/// <p/>This class can also be used, to generate lexicographically sortable (according
	/// {@link String#compareTo(String)}) representations of numeric data types for other
	/// usages (e.g. sorting).
	/// 
	/// <p/><font color="red"><b>NOTE:</b> This API is experimental and
	/// might change in incompatible ways in the next release.</font>
	/// 
	/// </summary>
	/// <since> 2.9
	/// </since>
	public sealed class NumericUtils
	{
		
		private NumericUtils()
		{
		} // no instance!
		
		/// <summary> The default precision step used by {@link NumericField}, {@link NumericTokenStream},
		/// {@link NumericRangeQuery}, and {@link NumericRangeFilter} as default
		/// </summary>
		public const int PRECISION_STEP_DEFAULT = 4;
		
		/// <summary> Expert: Longs are stored at lower precision by shifting off lower bits. The shift count is
		/// stored as <code>SHIFT_START_LONG+shift</code> in the first character
		/// </summary>
		public static char SHIFT_START_LONG = (char) 0x20;
		
		/// <summary> Expert: The maximum term length (used for <code>char[]</code> buffer size)
		/// for encoding <code>long</code> values.
		/// </summary>
		/// <seealso cref="LongToPrefixCoded(long,int,char[])">
		/// </seealso>
		public const int BUF_SIZE_LONG = 63 / 7 + 2;
		
		/// <summary> Expert: Integers are stored at lower precision by shifting off lower bits. The shift count is
		/// stored as <code>SHIFT_START_INT+shift</code> in the first character
		/// </summary>
		public static char SHIFT_START_INT = (char) 0x60;
		
		/// <summary> Expert: The maximum term length (used for <code>char[]</code> buffer size)
		/// for encoding <code>int</code> values.
		/// </summary>
		/// <seealso cref="IntToPrefixCoded(int,int,char[])">
		/// </seealso>
		public const int BUF_SIZE_INT = 31 / 7 + 2;
		
		/// <summary> Expert: Returns prefix coded bits after reducing the precision by <code>shift</code> bits.
		/// This is method is used by {@link NumericTokenStream}.
		/// </summary>
		/// <param name="val">the numeric value
		/// </param>
		/// <param name="shift">how many bits to strip from the right
		/// </param>
		/// <param name="buffer">that will contain the encoded chars, must be at least of {@link #BUF_SIZE_LONG}
		/// length
		/// </param>
		/// <returns> number of chars written to buffer
		/// </returns>
		public static int LongToPrefixCoded(long val, int shift, char[] buffer)
		{
			if (shift > 63 || shift < 0)
				throw new System.ArgumentException("Illegal shift value, must be 0..63");
			int nChars = (63 - shift) / 7 + 1, len = nChars + 1;
			buffer[0] = (char) (SHIFT_START_LONG + shift);
            ulong sortableBits = BitConverter.ToUInt64(BitConverter.GetBytes(val), 0) ^ 0x8000000000000000L;
			sortableBits = sortableBits >> shift;
			while (nChars >= 1)
			{
				// Store 7 bits per character for good efficiency when UTF-8 encoding.
				// The whole number is right-justified so that lucene can prefix-encode
				// the terms more efficiently.
				buffer[nChars--] = (char) (sortableBits & 0x7f);
				sortableBits = sortableBits >> 7;
			}
			return len;
		}
		
		/// <summary> Expert: Returns prefix coded bits after reducing the precision by <code>shift</code> bits.
		/// This is method is used by {@link LongRangeBuilder}.
		/// </summary>
		/// <param name="val">the numeric value
		/// </param>
		/// <param name="shift">how many bits to strip from the right
		/// </param>
		public static System.String LongToPrefixCoded(long val, int shift)
		{
			char[] buffer = new char[BUF_SIZE_LONG];
			int len = LongToPrefixCoded(val, shift, buffer);
			return new System.String(buffer, 0, len);
		}
		
		/// <summary> This is a convenience method, that returns prefix coded bits of a long without
		/// reducing the precision. It can be used to store the full precision value as a
		/// stored field in index.
		/// <p/>To decode, use {@link #prefixCodedToLong}.
		/// </summary>
		public static System.String LongToPrefixCoded(long val)
		{
			return LongToPrefixCoded(val, 0);
		}
		
		/// <summary> Expert: Returns prefix coded bits after reducing the precision by <code>shift</code> bits.
		/// This is method is used by {@link NumericTokenStream}.
		/// </summary>
		/// <param name="val">the numeric value
		/// </param>
		/// <param name="shift">how many bits to strip from the right
		/// </param>
		/// <param name="buffer">that will contain the encoded chars, must be at least of {@link #BUF_SIZE_INT}
		/// length
		/// </param>
		/// <returns> number of chars written to buffer
		/// </returns>
		public static int IntToPrefixCoded(int val, int shift, char[] buffer)
		{
			if (shift > 31 || shift < 0)
				throw new System.ArgumentException("Illegal shift value, must be 0..31");
			int nChars = (31 - shift) / 7 + 1, len = nChars + 1;
			buffer[0] = (char) (SHIFT_START_INT + shift);
			int sortableBits = val ^ unchecked((int) 0x80000000);
			sortableBits = SupportClass.Number.URShift(sortableBits, shift);
			while (nChars >= 1)
			{
				// Store 7 bits per character for good efficiency when UTF-8 encoding.
				// The whole number is right-justified so that lucene can prefix-encode
				// the terms more efficiently.
				buffer[nChars--] = (char) (sortableBits & 0x7f);
				sortableBits = SupportClass.Number.URShift(sortableBits, 7);
			}
			return len;
		}
		
		/// <summary> Expert: Returns prefix coded bits after reducing the precision by <code>shift</code> bits.
		/// This is method is used by {@link IntRangeBuilder}.
		/// </summary>
		/// <param name="val">the numeric value
		/// </param>
		/// <param name="shift">how many bits to strip from the right
		/// </param>
		public static System.String IntToPrefixCoded(int val, int shift)
		{
			char[] buffer = new char[BUF_SIZE_INT];
			int len = IntToPrefixCoded(val, shift, buffer);
			return new System.String(buffer, 0, len);
		}
		
		/// <summary> This is a convenience method, that returns prefix coded bits of an int without
		/// reducing the precision. It can be used to store the full precision value as a
		/// stored field in index.
		/// <p/>To decode, use {@link #prefixCodedToInt}.
		/// </summary>
		public static System.String IntToPrefixCoded(int val)
		{
			return IntToPrefixCoded(val, 0);
		}
		
		/// <summary> Returns a long from prefixCoded characters.
		/// Rightmost bits will be zero for lower precision codes.
		/// This method can be used to decode e.g. a stored field.
		/// </summary>
		/// <throws>  NumberFormatException if the supplied string is </throws>
		/// <summary> not correctly prefix encoded.
		/// </summary>
		/// <seealso cref="LongToPrefixCoded(long)">
		/// </seealso>
		public static long PrefixCodedToLong(System.String prefixCoded)
		{
			int shift = prefixCoded[0] - SHIFT_START_LONG;
			if (shift > 63 || shift < 0)
				throw new System.FormatException("Invalid shift value in prefixCoded string (is encoded value really a LONG?)");
			ulong sortableBits = 0UL;
			for (int i = 1, len = prefixCoded.Length; i < len; i++)
			{
				sortableBits <<= 7;
				char ch = prefixCoded[i];
				if (ch > 0x7f)
				{
					throw new System.FormatException("Invalid prefixCoded numerical value representation (char " + System.Convert.ToString((int) ch, 16) + " at position " + i + " is invalid)");
				}
				sortableBits |= (ulong) ch;
			}
			return BitConverter.ToInt64(BitConverter.GetBytes((sortableBits << shift) ^ 0x8000000000000000L), 0);
		}
		
		/// <summary> Returns an int from prefixCoded characters.
		/// Rightmost bits will be zero for lower precision codes.
		/// This method can be used to decode e.g. a stored field.
		/// </summary>
		/// <throws>  NumberFormatException if the supplied string is </throws>
		/// <summary> not correctly prefix encoded.
		/// </summary>
		/// <seealso cref="IntToPrefixCoded(int)">
		/// </seealso>
		public static int PrefixCodedToInt(System.String prefixCoded)
		{
			int shift = prefixCoded[0] - SHIFT_START_INT;
			if (shift > 31 || shift < 0)
				throw new System.FormatException("Invalid shift value in prefixCoded string (is encoded value really an INT?)");
			int sortableBits = 0;
			for (int i = 1, len = prefixCoded.Length; i < len; i++)
			{
				sortableBits <<= 7;
				char ch = prefixCoded[i];
				if (ch > 0x7f)
				{
					throw new System.FormatException("Invalid prefixCoded numerical value representation (char " + System.Convert.ToString((int) ch, 16) + " at position " + i + " is invalid)");
				}
				sortableBits |= (int) ch;
			}
			return (sortableBits << shift) ^ unchecked((int) 0x80000000);
		}
		
		/// <summary> Converts a <code>double</code> value to a sortable signed <code>long</code>.
		/// The value is converted by getting their IEEE 754 floating-point &quot;double format&quot;
		/// bit layout and then some bits are swapped, to be able to compare the result as long.
		/// By this the precision is not reduced, but the value can easily used as a long.
		/// </summary>
		/// <seealso cref="sortableLongToDouble">
		/// </seealso>
		public static long DoubleToSortableLong(double val)
		{
            long f = BitConverter.DoubleToInt64Bits(val);   // {{Aroush-2.9}} will this work the same as 'java.lang.Double.doubleToRawLongBits()'?
			if (f < 0)
				f ^= 0x7fffffffffffffffL;
			return f;
		}
		
		/// <summary> Convenience method: this just returns:
		/// longToPrefixCoded(doubleToSortableLong(val))
		/// </summary>
		public static System.String DoubleToPrefixCoded(double val)
		{
			return LongToPrefixCoded(DoubleToSortableLong(val));
		}
		
		/// <summary> Converts a sortable <code>long</code> back to a <code>double</code>.</summary>
		/// <seealso cref="doubleToSortableLong">
		/// </seealso>
		public static double SortableLongToDouble(long val)
		{
			if (val < 0)
				val ^= 0x7fffffffffffffffL;
			return BitConverter.Int64BitsToDouble(val);
		}
		
		/// <summary> Convenience method: this just returns:
		/// sortableLongToDouble(prefixCodedToLong(val))
		/// </summary>
		public static double PrefixCodedToDouble(System.String val)
		{
			return SortableLongToDouble(PrefixCodedToLong(val));
		}
		
		/// <summary> Converts a <code>float</code> value to a sortable signed <code>int</code>.
		/// The value is converted by getting their IEEE 754 floating-point &quot;float format&quot;
		/// bit layout and then some bits are swapped, to be able to compare the result as int.
		/// By this the precision is not reduced, but the value can easily used as an int.
		/// </summary>
		/// <seealso cref="sortableIntToFloat">
		/// </seealso>
		public static int FloatToSortableInt(float val)
		{
			int f = BitConverter.ToInt32(BitConverter.GetBytes(val), 0);
			if (f < 0)
				f ^= 0x7fffffff;
			return f;
		}
		
		/// <summary> Convenience method: this just returns:
		/// intToPrefixCoded(floatToSortableInt(val))
		/// </summary>
		public static System.String FloatToPrefixCoded(float val)
		{
			return IntToPrefixCoded(FloatToSortableInt(val));
		}
		
		/// <summary> Converts a sortable <code>int</code> back to a <code>float</code>.</summary>
		/// <seealso cref="floatToSortableInt">
		/// </seealso>
		public static float SortableIntToFloat(int val)
		{
			if (val < 0)
				val ^= 0x7fffffff;
            return BitConverter.ToSingle(BitConverter.GetBytes(val), 0);
		}
		
		/// <summary> Convenience method: this just returns:
		/// sortableIntToFloat(prefixCodedToInt(val))
		/// </summary>
		public static float PrefixCodedToFloat(System.String val)
		{
			return SortableIntToFloat(PrefixCodedToInt(val));
		}
		
		/// <summary> Expert: Splits a long range recursively.
		/// You may implement a builder that adds clauses to a
		/// {@link Mono.Lucene.Net.Search.BooleanQuery} for each call to its
		/// {@link LongRangeBuilder#AddRange(String,String)}
		/// method.
		/// <p/>This method is used by {@link NumericRangeQuery}.
		/// </summary>
		public static void  SplitLongRange(LongRangeBuilder builder, int precisionStep, long minBound, long maxBound)
		{
			SplitRange(builder, 64, precisionStep, minBound, maxBound);
		}
		
		/// <summary> Expert: Splits an int range recursively.
		/// You may implement a builder that adds clauses to a
		/// {@link Mono.Lucene.Net.Search.BooleanQuery} for each call to its
		/// {@link IntRangeBuilder#AddRange(String,String)}
		/// method.
		/// <p/>This method is used by {@link NumericRangeQuery}.
		/// </summary>
		public static void  SplitIntRange(IntRangeBuilder builder, int precisionStep, int minBound, int maxBound)
		{
			SplitRange(builder, 32, precisionStep, (long) minBound, (long) maxBound);
		}
		
		/// <summary>This helper does the splitting for both 32 and 64 bit. </summary>
		private static void  SplitRange(System.Object builder, int valSize, int precisionStep, long minBound, long maxBound)
		{
			if (precisionStep < 1)
				throw new System.ArgumentException("precisionStep must be >=1");
			if (minBound > maxBound)
				return ;
			for (int shift = 0; ; shift += precisionStep)
			{
				// calculate new bounds for inner precision
				long diff = 1L << (shift + precisionStep);
				long mask = ((1L << precisionStep) - 1L) << shift;
				bool hasLower = (minBound & mask) != 0L;
				bool hasUpper = (maxBound & mask) != mask;
				long nextMinBound = (hasLower?(minBound + diff):minBound) & ~ mask;
				long nextMaxBound = (hasUpper?(maxBound - diff):maxBound) & ~ mask;
				bool lowerWrapped = nextMinBound < minBound,
                     upperWrapped = nextMaxBound > maxBound;
      
                if (shift+precisionStep>=valSize || nextMinBound>nextMaxBound || lowerWrapped || upperWrapped) 
				{
					// We are in the lowest precision or the next precision is not available.
					AddRange(builder, valSize, minBound, maxBound, shift);
					// exit the split recursion loop
					break;
				}
				
				if (hasLower)
					AddRange(builder, valSize, minBound, minBound | mask, shift);
				if (hasUpper)
					AddRange(builder, valSize, maxBound & ~ mask, maxBound, shift);
				
				// recurse to next precision
				minBound = nextMinBound;
				maxBound = nextMaxBound;
			}
		}
		
		/// <summary>Helper that delegates to correct range builder </summary>
		private static void  AddRange(System.Object builder, int valSize, long minBound, long maxBound, int shift)
		{
			// for the max bound set all lower bits (that were shifted away):
			// this is important for testing or other usages of the splitted range
			// (e.g. to reconstruct the full range). The prefixEncoding will remove
			// the bits anyway, so they do not hurt!
			maxBound |= (1L << shift) - 1L;
			// delegate to correct range builder
			switch (valSize)
			{
				
				case 64: 
					((LongRangeBuilder) builder).AddRange(minBound, maxBound, shift);
					break;
				
				case 32: 
					((IntRangeBuilder) builder).AddRange((int) minBound, (int) maxBound, shift);
					break;
				
				default: 
					// Should not happen!
					throw new System.ArgumentException("valSize must be 32 or 64.");
				
			}
		}
		
		/// <summary> Expert: Callback for {@link #splitLongRange}.
		/// You need to overwrite only one of the methods.
		/// <p/><font color="red"><b>NOTE:</b> This is a very low-level interface,
		/// the method signatures may change in later versions.</font>
		/// </summary>
		public abstract class LongRangeBuilder
		{
			
			/// <summary> Overwrite this method, if you like to receive the already prefix encoded range bounds.
			/// You can directly build classical (inclusive) range queries from them.
			/// </summary>
			public virtual void  AddRange(System.String minPrefixCoded, System.String maxPrefixCoded)
			{
				throw new System.NotSupportedException();
			}
			
			/// <summary> Overwrite this method, if you like to receive the raw long range bounds.
			/// You can use this for e.g. debugging purposes (print out range bounds).
			/// </summary>
			public virtual void  AddRange(long min, long max, int shift)
			{
				AddRange(Mono.Lucene.Net.Util.NumericUtils.LongToPrefixCoded(min, shift), Mono.Lucene.Net.Util.NumericUtils.LongToPrefixCoded(max, shift));
			}
		}
		
		/// <summary> Expert: Callback for {@link #splitIntRange}.
		/// You need to overwrite only one of the methods.
		/// <p/><font color="red"><b>NOTE:</b> This is a very low-level interface,
		/// the method signatures may change in later versions.</font>
		/// </summary>
		public abstract class IntRangeBuilder
		{
			
			/// <summary> Overwrite this method, if you like to receive the already prefix encoded range bounds.
			/// You can directly build classical range (inclusive) queries from them.
			/// </summary>
			public virtual void  AddRange(System.String minPrefixCoded, System.String maxPrefixCoded)
			{
				throw new System.NotSupportedException();
			}
			
			/// <summary> Overwrite this method, if you like to receive the raw int range bounds.
			/// You can use this for e.g. debugging purposes (print out range bounds).
			/// </summary>
			public virtual void  AddRange(int min, int max, int shift)
			{
				AddRange(Mono.Lucene.Net.Util.NumericUtils.IntToPrefixCoded(min, shift), Mono.Lucene.Net.Util.NumericUtils.IntToPrefixCoded(max, shift));
			}
		}
	}
}
