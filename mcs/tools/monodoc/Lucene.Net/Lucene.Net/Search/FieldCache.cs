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
using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using NumericUtils = Mono.Lucene.Net.Util.NumericUtils;
using RamUsageEstimator = Mono.Lucene.Net.Util.RamUsageEstimator;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Expert: Maintains caches of term values.
	/// 
	/// <p/>Created: May 19, 2004 11:13:14 AM
	/// 
	/// </summary>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: FieldCache.java 807841 2009-08-25 22:27:31Z markrmiller $
	/// </version>
	/// <seealso cref="Mono.Lucene.Net.Util.FieldCacheSanityChecker">
	/// </seealso>
	public sealed class CreationPlaceholder
	{
		internal System.Object value_Renamed;
	}
	/// <summary>Expert: Stores term text values and document ordering data. </summary>
	public class StringIndex
	{
		
		public virtual int BinarySearchLookup(System.String key)
		{
			// this special case is the reason that Arrays.binarySearch() isn't useful.
			if (key == null)
				return 0;
			
			int low = 1;
			int high = lookup.Length - 1;
			
			while (low <= high)
			{
				int mid = SupportClass.Number.URShift((low + high), 1);
				int cmp = String.CompareOrdinal(lookup[mid], key);
				
				if (cmp < 0)
					low = mid + 1;
				else if (cmp > 0)
					high = mid - 1;
				else
					return mid; // key found
			}
			return - (low + 1); // key not found.
		}
		
		/// <summary>All the term values, in natural order. </summary>
		public System.String[] lookup;
		
		/// <summary>For each document, an index into the lookup array. </summary>
		public int[] order;
		
		/// <summary>Creates one of these objects </summary>
		public StringIndex(int[] values, System.String[] lookup)
		{
			this.order = values;
			this.lookup = lookup;
		}
	}
	/// <summary> EXPERT: A unique Identifier/Description for each item in the FieldCache. 
	/// Can be useful for logging/debugging.
	/// <p/>
	/// <b>EXPERIMENTAL API:</b> This API is considered extremely advanced 
	/// and experimental.  It may be removed or altered w/o warning in future 
	/// releases 
	/// of Lucene.
	/// <p/>
	/// </summary>
	public abstract class CacheEntry
	{
		public abstract System.Object GetReaderKey();
		public abstract System.String GetFieldName();
		public abstract System.Type GetCacheType();
		public abstract System.Object GetCustom();
		public abstract System.Object GetValue();
		private System.String size = null;
		protected internal void  SetEstimatedSize(System.String size)
		{
			this.size = size;
		}
		/// <seealso cref="EstimateSize(RamUsageEstimator)">
		/// </seealso>
		public virtual void  EstimateSize()
		{
			EstimateSize(new RamUsageEstimator(false)); // doesn't check for interned
		}
		/// <summary> Computes (and stores) the estimated size of the cache Value </summary>
		/// <seealso cref="getEstimatedSize">
		/// </seealso>
		public virtual void  EstimateSize(RamUsageEstimator ramCalc)
		{
			long size = ramCalc.EstimateRamUsage(GetValue());
            SetEstimatedSize(RamUsageEstimator.HumanReadableUnits(size, new System.Globalization.NumberFormatInfo()));  // {{Aroush-2.9}} in Java, the formater is set to "0.#", so we need to do the same in C#
		}
		/// <summary> The most recently estimated size of the value, null unless 
		/// estimateSize has been called.
		/// </summary>
		public System.String GetEstimatedSize()
		{
			return size;
		}
		
		
		public override System.String ToString()
		{
			System.Text.StringBuilder b = new System.Text.StringBuilder();
			b.Append("'").Append(GetReaderKey()).Append("'=>");
			b.Append("'").Append(GetFieldName()).Append("',");
			b.Append(GetCacheType()).Append(",").Append(GetCustom());
			b.Append("=>").Append(GetValue().GetType().FullName).Append("#");
			b.Append(GetValue().GetHashCode());
			
			System.String s = GetEstimatedSize();
			if (null != s)
			{
				b.Append(" (size =~ ").Append(s).Append(')');
			}
			
			return b.ToString();
		}
	}
	public struct FieldCache_Fields{
		/// <summary>Indicator for StringIndex values in the cache. </summary>
		// NOTE: the value assigned to this constant must not be
		// the same as any of those in SortField!!
		public readonly static int STRING_INDEX = - 1;
		/// <summary>Expert: The cache used internally by sorting and range query classes. </summary>
		public readonly static FieldCache DEFAULT;
		/// <summary>The default parser for byte values, which are encoded by {@link Byte#toString(byte)} </summary>
		public readonly static ByteParser DEFAULT_BYTE_PARSER;
		/// <summary>The default parser for short values, which are encoded by {@link Short#toString(short)} </summary>
		public readonly static ShortParser DEFAULT_SHORT_PARSER;
		/// <summary>The default parser for int values, which are encoded by {@link Integer#toString(int)} </summary>
		public readonly static IntParser DEFAULT_INT_PARSER;
		/// <summary>The default parser for float values, which are encoded by {@link Float#toString(float)} </summary>
		public readonly static FloatParser DEFAULT_FLOAT_PARSER;
		/// <summary>The default parser for long values, which are encoded by {@link Long#toString(long)} </summary>
		public readonly static LongParser DEFAULT_LONG_PARSER;
		/// <summary>The default parser for double values, which are encoded by {@link Double#toString(double)} </summary>
		public readonly static DoubleParser DEFAULT_DOUBLE_PARSER;
		/// <summary> A parser instance for int values encoded by {@link NumericUtils#IntToPrefixCoded(int)}, e.g. when indexed
		/// via {@link NumericField}/{@link NumericTokenStream}.
		/// </summary>
		public readonly static IntParser NUMERIC_UTILS_INT_PARSER;
		/// <summary> A parser instance for float values encoded with {@link NumericUtils}, e.g. when indexed
		/// via {@link NumericField}/{@link NumericTokenStream}.
		/// </summary>
		public readonly static FloatParser NUMERIC_UTILS_FLOAT_PARSER;
		/// <summary> A parser instance for long values encoded by {@link NumericUtils#LongToPrefixCoded(long)}, e.g. when indexed
		/// via {@link NumericField}/{@link NumericTokenStream}.
		/// </summary>
		public readonly static LongParser NUMERIC_UTILS_LONG_PARSER;
		/// <summary> A parser instance for double values encoded with {@link NumericUtils}, e.g. when indexed
		/// via {@link NumericField}/{@link NumericTokenStream}.
		/// </summary>
		public readonly static DoubleParser NUMERIC_UTILS_DOUBLE_PARSER;
		static FieldCache_Fields()
		{
			DEFAULT = new FieldCacheImpl();
			DEFAULT_BYTE_PARSER = new AnonymousClassByteParser();
			DEFAULT_SHORT_PARSER = new AnonymousClassShortParser();
			DEFAULT_INT_PARSER = new AnonymousClassIntParser();
			DEFAULT_FLOAT_PARSER = new AnonymousClassFloatParser();
			DEFAULT_LONG_PARSER = new AnonymousClassLongParser();
			DEFAULT_DOUBLE_PARSER = new AnonymousClassDoubleParser();
			NUMERIC_UTILS_INT_PARSER = new AnonymousClassIntParser1();
			NUMERIC_UTILS_FLOAT_PARSER = new AnonymousClassFloatParser1();
			NUMERIC_UTILS_LONG_PARSER = new AnonymousClassLongParser1();
			NUMERIC_UTILS_DOUBLE_PARSER = new AnonymousClassDoubleParser1();
		}
	}
    
	[Serializable]
	class AnonymousClassByteParser : ByteParser
	{
		public virtual sbyte ParseByte(System.String value_Renamed)
		{
            return System.SByte.Parse(value_Renamed);
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_BYTE_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".DEFAULT_BYTE_PARSER";
		}
	}
	[Serializable]
	class AnonymousClassShortParser : ShortParser
	{
		public virtual short ParseShort(System.String value_Renamed)
		{
			return System.Int16.Parse(value_Renamed);
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_SHORT_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".DEFAULT_SHORT_PARSER";
		}
	}
	[Serializable]
	class AnonymousClassIntParser : IntParser
	{
		public virtual int ParseInt(System.String value_Renamed)
		{
			return System.Int32.Parse(value_Renamed);
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_INT_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".DEFAULT_INT_PARSER";
		}
	}
	[Serializable]
	class AnonymousClassFloatParser : FloatParser
	{
		public virtual float ParseFloat(System.String value_Renamed)
		{
            try
            {
                return SupportClass.Single.Parse(value_Renamed);
            }
            catch (System.OverflowException)
            {
                return value_Renamed.StartsWith("-") ? float.PositiveInfinity : float.NegativeInfinity;
            }
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_FLOAT_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".DEFAULT_FLOAT_PARSER";
		}
	}
	[Serializable]
	class AnonymousClassLongParser : LongParser
	{
		public virtual long ParseLong(System.String value_Renamed)
		{
			return System.Int64.Parse(value_Renamed);
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_LONG_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".DEFAULT_LONG_PARSER";
		}
	}
	[Serializable]
	class AnonymousClassDoubleParser : DoubleParser
	{
		public virtual double ParseDouble(System.String value_Renamed)
		{
			return SupportClass.Double.Parse(value_Renamed);
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_DOUBLE_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".DEFAULT_DOUBLE_PARSER";
		}
	}
	[Serializable]
	class AnonymousClassIntParser1 : IntParser
	{
		public virtual int ParseInt(System.String val)
		{
			int shift = val[0] - NumericUtils.SHIFT_START_INT;
			if (shift > 0 && shift <= 31)
				throw new FieldCacheImpl.StopFillCacheException();
			return NumericUtils.PrefixCodedToInt(val);
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.NUMERIC_UTILS_INT_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".NUMERIC_UTILS_INT_PARSER";
		}
	}
	[Serializable]
	class AnonymousClassFloatParser1 : FloatParser
	{
		public virtual float ParseFloat(System.String val)
		{
			int shift = val[0] - NumericUtils.SHIFT_START_INT;
			if (shift > 0 && shift <= 31)
				throw new FieldCacheImpl.StopFillCacheException();
			return NumericUtils.SortableIntToFloat(NumericUtils.PrefixCodedToInt(val));
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.NUMERIC_UTILS_FLOAT_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".NUMERIC_UTILS_FLOAT_PARSER";
		}
	}
	[Serializable]
	class AnonymousClassLongParser1 : LongParser
	{
		public virtual long ParseLong(System.String val)
		{
			int shift = val[0] - NumericUtils.SHIFT_START_LONG;
			if (shift > 0 && shift <= 63)
				throw new FieldCacheImpl.StopFillCacheException();
			return NumericUtils.PrefixCodedToLong(val);
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.NUMERIC_UTILS_LONG_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".NUMERIC_UTILS_LONG_PARSER";
		}
	}
	[Serializable]
	class AnonymousClassDoubleParser1 : DoubleParser
	{
		public virtual double ParseDouble(System.String val)
		{
			int shift = val[0] - NumericUtils.SHIFT_START_LONG;
			if (shift > 0 && shift <= 63)
				throw new FieldCacheImpl.StopFillCacheException();
			return NumericUtils.SortableLongToDouble(NumericUtils.PrefixCodedToLong(val));
		}
		protected internal virtual System.Object ReadResolve()
		{
			return Mono.Lucene.Net.Search.FieldCache_Fields.NUMERIC_UTILS_DOUBLE_PARSER;
		}
		public override System.String ToString()
		{
			return typeof(FieldCache).FullName + ".NUMERIC_UTILS_DOUBLE_PARSER";
		}
	}
	public interface FieldCache
	{
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none is
		/// found, reads the terms in <code>field</code> as a single byte and returns an array
		/// of size <code>reader.maxDoc()</code> of the value each document
		/// has in the given field.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the single byte values.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		sbyte[] GetBytes(IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none is found,
		/// reads the terms in <code>field</code> as bytes and returns an array of
		/// size <code>reader.maxDoc()</code> of the value each document has in the
		/// given field.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the bytes.
		/// </param>
		/// <param name="parser"> Computes byte for string values.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		sbyte[] GetBytes(IndexReader reader, System.String field, ByteParser parser);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none is
		/// found, reads the terms in <code>field</code> as shorts and returns an array
		/// of size <code>reader.maxDoc()</code> of the value each document
		/// has in the given field.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the shorts.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		short[] GetShorts(IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none is found,
		/// reads the terms in <code>field</code> as shorts and returns an array of
		/// size <code>reader.maxDoc()</code> of the value each document has in the
		/// given field.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the shorts.
		/// </param>
		/// <param name="parser"> Computes short for string values.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		short[] GetShorts(IndexReader reader, System.String field, ShortParser parser);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none is
		/// found, reads the terms in <code>field</code> as integers and returns an array
		/// of size <code>reader.maxDoc()</code> of the value each document
		/// has in the given field.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the integers.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		int[] GetInts(IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none is found,
		/// reads the terms in <code>field</code> as integers and returns an array of
		/// size <code>reader.maxDoc()</code> of the value each document has in the
		/// given field.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the integers.
		/// </param>
		/// <param name="parser"> Computes integer for string values.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		int[] GetInts(IndexReader reader, System.String field, IntParser parser);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if
		/// none is found, reads the terms in <code>field</code> as floats and returns an array
		/// of size <code>reader.maxDoc()</code> of the value each document
		/// has in the given field.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the floats.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		float[] GetFloats(IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if
		/// none is found, reads the terms in <code>field</code> as floats and returns an array
		/// of size <code>reader.maxDoc()</code> of the value each document
		/// has in the given field.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the floats.
		/// </param>
		/// <param name="parser"> Computes float for string values.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		float[] GetFloats(IndexReader reader, System.String field, FloatParser parser);
		
		/// <summary> Checks the internal cache for an appropriate entry, and if none is
		/// found, reads the terms in <code>field</code> as longs and returns an array
		/// of size <code>reader.maxDoc()</code> of the value each document
		/// has in the given field.
		/// 
		/// </summary>
		/// <param name="reader">Used to get field values.
		/// </param>
		/// <param name="field"> Which field contains the longs.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  java.io.IOException If any error occurs. </throws>
		long[] GetLongs(IndexReader reader, System.String field);
		
		/// <summary> Checks the internal cache for an appropriate entry, and if none is found,
		/// reads the terms in <code>field</code> as longs and returns an array of
		/// size <code>reader.maxDoc()</code> of the value each document has in the
		/// given field.
		/// 
		/// </summary>
		/// <param name="reader">Used to get field values.
		/// </param>
		/// <param name="field"> Which field contains the longs.
		/// </param>
		/// <param name="parser">Computes integer for string values.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException If any error occurs. </throws>
		long[] GetLongs(IndexReader reader, System.String field, LongParser parser);
		
		
		/// <summary> Checks the internal cache for an appropriate entry, and if none is
		/// found, reads the terms in <code>field</code> as integers and returns an array
		/// of size <code>reader.maxDoc()</code> of the value each document
		/// has in the given field.
		/// 
		/// </summary>
		/// <param name="reader">Used to get field values.
		/// </param>
		/// <param name="field"> Which field contains the doubles.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException If any error occurs. </throws>
		double[] GetDoubles(IndexReader reader, System.String field);
		
		/// <summary> Checks the internal cache for an appropriate entry, and if none is found,
		/// reads the terms in <code>field</code> as doubles and returns an array of
		/// size <code>reader.maxDoc()</code> of the value each document has in the
		/// given field.
		/// 
		/// </summary>
		/// <param name="reader">Used to get field values.
		/// </param>
		/// <param name="field"> Which field contains the doubles.
		/// </param>
		/// <param name="parser">Computes integer for string values.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException If any error occurs. </throws>
		double[] GetDoubles(IndexReader reader, System.String field, DoubleParser parser);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none
		/// is found, reads the term values in <code>field</code> and returns an array
		/// of size <code>reader.maxDoc()</code> containing the value each document
		/// has in the given field.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the strings.
		/// </param>
		/// <returns> The values in the given field for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		System.String[] GetStrings(IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none
		/// is found reads the term values in <code>field</code> and returns
		/// an array of them in natural order, along with an array telling
		/// which element in the term array each document uses.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the strings.
		/// </param>
		/// <returns> Array of terms and index into the array for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		StringIndex GetStringIndex(IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if
		/// none is found reads <code>field</code> to see if it contains integers, longs, floats
		/// or strings, and then calls one of the other methods in this class to get the
		/// values.  For string values, a StringIndex is returned.  After
		/// calling this method, there is an entry in the cache for both
		/// type <code>AUTO</code> and the actual found type.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the values.
		/// </param>
		/// <returns> int[], long[], float[] or StringIndex.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		/// <deprecated> Please specify the exact type, instead.
		/// Especially, guessing does <b>not</b> work with the new
		/// {@link NumericField} type.
		/// </deprecated>
        [Obsolete("Please specify the exact type, instead. Especially, guessing does not work with the new NumericField type.")]
		System.Object GetAuto(IndexReader reader, System.String field);
		
		/// <summary>Checks the internal cache for an appropriate entry, and if none
		/// is found reads the terms out of <code>field</code> and calls the given SortComparator
		/// to get the sort values.  A hit in the cache will happen if <code>reader</code>,
		/// <code>field</code>, and <code>comparator</code> are the same (using <code>equals()</code>)
		/// as a previous call to this method.
		/// </summary>
		/// <param name="reader"> Used to get field values.
		/// </param>
		/// <param name="field">  Which field contains the values.
		/// </param>
		/// <param name="comparator">Used to convert terms into something to sort by.
		/// </param>
		/// <returns> Array of sort objects, one for each document.
		/// </returns>
		/// <throws>  IOException  If any error occurs. </throws>
		/// <deprecated> Please implement {@link
		/// FieldComparatorSource} directly, instead.
		/// </deprecated>
        [Obsolete("Please implement FieldComparatorSource directly, instead.")]
		System.IComparable[] GetCustom(IndexReader reader, System.String field, SortComparator comparator);
		
		/// <summary> EXPERT: Generates an array of CacheEntry objects representing all items 
		/// currently in the FieldCache.
		/// <p/>
		/// NOTE: These CacheEntry objects maintain a strong refrence to the 
		/// Cached Values.  Maintaining refrences to a CacheEntry the IndexReader 
		/// associated with it has garbage collected will prevent the Value itself
		/// from being garbage collected when the Cache drops the WeakRefrence.
		/// <p/>
		/// <p/>
		/// <b>EXPERIMENTAL API:</b> This API is considered extremely advanced 
		/// and experimental.  It may be removed or altered w/o warning in future 
		/// releases 
		/// of Lucene.
		/// <p/>
		/// </summary>
		CacheEntry[] GetCacheEntries();
		
		/// <summary> <p/>
		/// EXPERT: Instructs the FieldCache to forcibly expunge all entries 
		/// from the underlying caches.  This is intended only to be used for 
		/// test methods as a way to ensure a known base state of the Cache 
		/// (with out needing to rely on GC to free WeakReferences).  
		/// It should not be relied on for "Cache maintenance" in general 
		/// application code.
		/// <p/>
		/// <p/>
		/// <b>EXPERIMENTAL API:</b> This API is considered extremely advanced 
		/// and experimental.  It may be removed or altered w/o warning in future 
		/// releases 
		/// of Lucene.
		/// <p/>
		/// </summary>
		void  PurgeAllCaches();

        /// <summary>
        /// Expert: drops all cache entries associated with this
        /// reader.  NOTE: this reader must precisely match the
        /// reader that the cache entry is keyed on. If you pass a
        /// top-level reader, it usually will have no effect as
        /// Lucene now caches at the segment reader level.
        /// </summary>
        void Purge(IndexReader r);
		
		/// <summary> If non-null, FieldCacheImpl will warn whenever
		/// entries are created that are not sane according to
		/// {@link Mono.Lucene.Net.Util.FieldCacheSanityChecker}.
		/// </summary>
		void  SetInfoStream(System.IO.StreamWriter stream);
		
		/// <summary>counterpart of {@link #SetInfoStream(PrintStream)} </summary>
		System.IO.StreamWriter GetInfoStream();
	}
	
	/// <summary> Marker interface as super-interface to all parsers. It
	/// is used to specify a custom parser to {@link
	/// SortField#SortField(String, FieldCache.Parser)}.
	/// </summary>
	public interface Parser
	{
	}
	
	/// <summary>Interface to parse bytes from document fields.</summary>
	/// <seealso cref="FieldCache.GetBytes(IndexReader, String, FieldCache.ByteParser)">
	/// </seealso>
	public interface ByteParser:Parser
	{
		/// <summary>Return a single Byte representation of this field's value. </summary>
		sbyte ParseByte(System.String string_Renamed);
	}
	
	/// <summary>Interface to parse shorts from document fields.</summary>
	/// <seealso cref="FieldCache.GetShorts(IndexReader, String, FieldCache.ShortParser)">
	/// </seealso>
	public interface ShortParser:Parser
	{
		/// <summary>Return a short representation of this field's value. </summary>
		short ParseShort(System.String string_Renamed);
	}
	
	/// <summary>Interface to parse ints from document fields.</summary>
	/// <seealso cref="FieldCache.GetInts(IndexReader, String, FieldCache.IntParser)">
	/// </seealso>
	public interface IntParser:Parser
	{
		/// <summary>Return an integer representation of this field's value. </summary>
		int ParseInt(System.String string_Renamed);
	}
	
	/// <summary>Interface to parse floats from document fields.</summary>
	/// <seealso cref="FieldCache.GetFloats(IndexReader, String, FieldCache.FloatParser)">
	/// </seealso>
	public interface FloatParser:Parser
	{
		/// <summary>Return an float representation of this field's value. </summary>
		float ParseFloat(System.String string_Renamed);
	}
	
	/// <summary>Interface to parse long from document fields.</summary>
	/// <seealso cref="FieldCache.GetLongs(IndexReader, String, FieldCache.LongParser)">
	/// </seealso>
	/// <deprecated> Use {@link FieldCache.LongParser}, this will be removed in Lucene 3.0 
	/// </deprecated>
    [Obsolete("Use FieldCache.LongParser, this will be removed in Lucene 3.0")]
	public interface LongParser:Parser
	{
		/// <summary>Return an long representation of this field's value. </summary>
		long ParseLong(System.String string_Renamed);
	}
	
	/// <summary>Interface to parse doubles from document fields.</summary>
	/// <seealso cref="FieldCache.GetDoubles(IndexReader, String, FieldCache.DoubleParser)">
	/// </seealso>
	/// <deprecated> Use {@link FieldCache.DoubleParser}, this will be removed in Lucene 3.0 
	/// </deprecated>
    [Obsolete("Use FieldCache.DoubleParser, this will be removed in Lucene 3.0 ")]
	public interface DoubleParser:Parser
	{
		/// <summary>Return an long representation of this field's value. </summary>
		double ParseDouble(System.String string_Renamed);
	}
}
