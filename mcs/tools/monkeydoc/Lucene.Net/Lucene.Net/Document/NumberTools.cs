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

using NumericUtils = Mono.Lucene.Net.Util.NumericUtils;
using NumericRangeQuery = Mono.Lucene.Net.Search.NumericRangeQuery;

namespace Mono.Lucene.Net.Documents
{
	
	// do not remove this class in 3.0, it may be needed to decode old indexes!
	
	/// <summary> Provides support for converting longs to Strings, and back again. The strings
	/// are structured so that lexicographic sorting order is preserved.
	/// 
	/// <p/>
	/// That is, if l1 is less than l2 for any two longs l1 and l2, then
	/// NumberTools.longToString(l1) is lexicographically less than
	/// NumberTools.longToString(l2). (Similarly for "greater than" and "equals".)
	/// 
	/// <p/>
	/// This class handles <b>all</b> long values (unlike
	/// {@link Mono.Lucene.Net.Documents.DateField}).
	/// 
	/// </summary>
	/// <deprecated> For new indexes use {@link NumericUtils} instead, which
	/// provides a sortable binary representation (prefix encoded) of numeric
	/// values.
	/// To index and efficiently query numeric values use {@link NumericField}
	/// and {@link NumericRangeQuery}.
	/// This class is included for use with existing
	/// indices and will be removed in a future release.
	/// </deprecated>
    [Obsolete("For new indexes use NumericUtils instead, which provides a sortable binary representation (prefix encoded) of numeric values. To index and efficiently query numeric values use NumericField and NumericRangeQuery. This class is included for use with existing indices and will be removed in a future release.")]
	public class NumberTools
	{
		
		private const int RADIX = 36;
		
		private const char NEGATIVE_PREFIX = '-';
		
		// NB: NEGATIVE_PREFIX must be < POSITIVE_PREFIX
		private const char POSITIVE_PREFIX = '0';
		
		//NB: this must be less than
		/// <summary> Equivalent to longToString(Long.MIN_VALUE)</summary>
#if !PRE_LUCENE_NET_2_0_0_COMPATIBLE
		public static readonly System.String MIN_STRING_VALUE = NEGATIVE_PREFIX + "0000000000000";
#else
        public static readonly System.String MIN_STRING_VALUE = NEGATIVE_PREFIX + "0000000000000000";
#endif
		
		/// <summary> Equivalent to longToString(Long.MAX_VALUE)</summary>
#if !PRE_LUCENE_NET_2_0_0_COMPATIBLE
		public static readonly System.String MAX_STRING_VALUE = POSITIVE_PREFIX + "1y2p0ij32e8e7";
#else
        public static readonly System.String MAX_STRING_VALUE = POSITIVE_PREFIX + "7fffffffffffffff";
#endif
		
		/// <summary> The length of (all) strings returned by {@link #longToString}</summary>
		public static readonly int STR_SIZE = MIN_STRING_VALUE.Length;
		
		/// <summary> Converts a long to a String suitable for indexing.</summary>
		public static System.String LongToString(long l)
		{
			
			if (l == System.Int64.MinValue)
			{
				// special case, because long is not symmetric around zero
				return MIN_STRING_VALUE;
			}
			
			System.Text.StringBuilder buf = new System.Text.StringBuilder(STR_SIZE);
			
			if (l < 0)
			{
				buf.Append(NEGATIVE_PREFIX);
				l = System.Int64.MaxValue + l + 1;
			}
			else
			{
				buf.Append(POSITIVE_PREFIX);
			}
#if !PRE_LUCENE_NET_2_0_0_COMPATIBLE
            System.String num = ToString(l);
#else
            System.String num = System.Convert.ToString(l, RADIX);
#endif
			
			int padLen = STR_SIZE - num.Length - buf.Length;
			while (padLen-- > 0)
			{
				buf.Append('0');
			}
			buf.Append(num);
			
			return buf.ToString();
		}
		
		/// <summary> Converts a String that was returned by {@link #longToString} back to a
		/// long.
		/// 
		/// </summary>
		/// <throws>  IllegalArgumentException </throws>
		/// <summary>             if the input is null
		/// </summary>
		/// <throws>  NumberFormatException </throws>
		/// <summary>             if the input does not parse (it was not a String returned by
		/// longToString()).
		/// </summary>
		public static long StringToLong(System.String str)
		{
			if (str == null)
			{
				throw new System.NullReferenceException("string cannot be null");
			}
			if (str.Length != STR_SIZE)
			{
				throw new System.FormatException("string is the wrong size");
			}
			
			if (str.Equals(MIN_STRING_VALUE))
			{
				return System.Int64.MinValue;
			}
			
			char prefix = str[0];
#if !PRE_LUCENE_NET_2_0_0_COMPATIBLE
			long l = ToLong(str.Substring(1));
#else
            long l = System.Convert.ToInt64(str.Substring(1), RADIX);
#endif
			
			if (prefix == POSITIVE_PREFIX)
			{
				// nop
			}
			else if (prefix == NEGATIVE_PREFIX)
			{
				l = l - System.Int64.MaxValue - 1;
			}
			else
			{
				throw new System.FormatException("string does not begin with the correct prefix");
			}
			
			return l;
		}

#if !PRE_LUCENE_NET_2_0_0_COMPATIBLE
        #region BASE36 OPS 
        static System.String digits = "0123456789abcdefghijklmnopqrstuvwxyz";
        static long[] powersOf36 = 
            {
                1L,
                36L,
                36L*36L,
                36L*36L*36L,
                36L*36L*36L*36L,
                36L*36L*36L*36L*36L,
                36L*36L*36L*36L*36L*36L,
                36L*36L*36L*36L*36L*36L*36L,
                36L*36L*36L*36L*36L*36L*36L*36L,
                36L*36L*36L*36L*36L*36L*36L*36L*36L,
                36L*36L*36L*36L*36L*36L*36L*36L*36L*36L,
                36L*36L*36L*36L*36L*36L*36L*36L*36L*36L*36L,
                36L*36L*36L*36L*36L*36L*36L*36L*36L*36L*36L*36L
            };

        public static System.String ToString(long lval)
        {
            if (lval == 0)
            {
                return "0";
            }

            int maxStrLen = powersOf36.Length;
            long curval = lval;

            char[] tb = new char[maxStrLen];
            int outpos = 0;
            for (int i = 0; i < maxStrLen; i++)
            {
                long pval = powersOf36[maxStrLen - i - 1];
                int pos = (int)(curval / pval);
                tb[outpos++] = digits.Substring(pos, 1).ToCharArray()[0];
                curval = curval % pval;
            }
            if (outpos == 0)
                tb[outpos++] = '0';
            return new System.String(tb, 0, outpos).TrimStart('0');
        }

        public static long ToLong(System.String t)
        {
            long ival = 0;
            char[] tb = t.ToCharArray();
            for (int i = 0; i < tb.Length; i++)
            {
                ival += powersOf36[i] * digits.IndexOf(tb[tb.Length - i - 1]);
            }
            return ival;
        }
        #endregion
#endif
	}
}
