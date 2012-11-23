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

using NumericField = Mono.Lucene.Net.Documents.NumericField;
using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using TermDocs = Mono.Lucene.Net.Index.TermDocs;
using NumericUtils = Mono.Lucene.Net.Util.NumericUtils;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A range filter built on top of a cached single term field (in {@link FieldCache}).
	/// 
	/// <p/>FieldCacheRangeFilter builds a single cache for the field the first time it is used.
	/// Each subsequent FieldCacheRangeFilter on the same field then reuses this cache,
	/// even if the range itself changes. 
	/// 
	/// <p/>This means that FieldCacheRangeFilter is much faster (sometimes more than 100x as fast) 
	/// as building a {@link TermRangeFilter} (or {@link ConstantScoreRangeQuery} on a {@link TermRangeFilter})
	/// for each query, if using a {@link #newStringRange}. However, if the range never changes it
	/// is slower (around 2x as slow) than building a CachingWrapperFilter on top of a single TermRangeFilter.
	/// 
	/// For numeric data types, this filter may be significantly faster than {@link NumericRangeFilter}.
	/// Furthermore, it does not need the numeric values encoded by {@link NumericField}. But
	/// it has the problem that it only works with exact one value/document (see below).
	/// 
	/// <p/>As with all {@link FieldCache} based functionality, FieldCacheRangeFilter is only valid for 
	/// fields which exact one term for each document (except for {@link #newStringRange}
	/// where 0 terms are also allowed). Due to a restriction of {@link FieldCache}, for numeric ranges
	/// all terms that do not have a numeric value, 0 is assumed.
	/// 
	/// <p/>Thus it works on dates, prices and other single value fields but will not work on
	/// regular text fields. It is preferable to use a <code>NOT_ANALYZED</code> field to ensure that
	/// there is only a single term. 
	/// 
	/// <p/>This class does not have an constructor, use one of the static factory methods available,
	/// that create a correct instance for different data types supported by {@link FieldCache}.
	/// </summary>
	
	[Serializable]
	public abstract class FieldCacheRangeFilter:Filter
	{
		[Serializable]
		private class AnonymousClassFieldCacheRangeFilter:FieldCacheRangeFilter
		{
			private class AnonymousClassFieldCacheDocIdSet:FieldCacheDocIdSet
			{
				private void  InitBlock(Mono.Lucene.Net.Search.StringIndex fcsi, int inclusiveLowerPoint, int inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter enclosingInstance)
				{
					this.fcsi = fcsi;
					this.inclusiveLowerPoint = inclusiveLowerPoint;
					this.inclusiveUpperPoint = inclusiveUpperPoint;
					this.enclosingInstance = enclosingInstance;
				}
				private Mono.Lucene.Net.Search.StringIndex fcsi;
				private int inclusiveLowerPoint;
				private int inclusiveUpperPoint;
				private AnonymousClassFieldCacheRangeFilter enclosingInstance;
				public AnonymousClassFieldCacheRangeFilter Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				internal AnonymousClassFieldCacheDocIdSet(Mono.Lucene.Net.Search.StringIndex fcsi, int inclusiveLowerPoint, int inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter enclosingInstance, Mono.Lucene.Net.Index.IndexReader Param1, bool Param2):base(Param1, Param2)
				{
					InitBlock(fcsi, inclusiveLowerPoint, inclusiveUpperPoint, enclosingInstance);
				}
				internal override bool MatchDoc(int doc)
				{
					return fcsi.order[doc] >= inclusiveLowerPoint && fcsi.order[doc] <= inclusiveUpperPoint;
				}
			}
			internal AnonymousClassFieldCacheRangeFilter(System.String Param1, Mono.Lucene.Net.Search.Parser Param2, System.Object Param3, System.Object Param4, bool Param5, bool Param6):base(Param1, Param2, Param3, Param4, Param5, Param6)
			{
			}
			public override DocIdSet GetDocIdSet(IndexReader reader)
			{
				Mono.Lucene.Net.Search.StringIndex fcsi = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetStringIndex(reader, field);
				int lowerPoint = fcsi.BinarySearchLookup((System.String) lowerVal);
				int upperPoint = fcsi.BinarySearchLookup((System.String) upperVal);
				
				int inclusiveLowerPoint;
				int inclusiveUpperPoint;
				
				// Hints:
				// * binarySearchLookup returns 0, if value was null.
				// * the value is <0 if no exact hit was found, the returned value
				//   is (-(insertion point) - 1)
				if (lowerPoint == 0)
				{
					System.Diagnostics.Debug.Assert(lowerVal == null);
					inclusiveLowerPoint = 1;
				}
				else if (includeLower && lowerPoint > 0)
				{
					inclusiveLowerPoint = lowerPoint;
				}
				else if (lowerPoint > 0)
				{
					inclusiveLowerPoint = lowerPoint + 1;
				}
				else
				{
					inclusiveLowerPoint = System.Math.Max(1, - lowerPoint - 1);
				}
				
				if (upperPoint == 0)
				{
					System.Diagnostics.Debug.Assert(upperVal == null);
					inclusiveUpperPoint = System.Int32.MaxValue;
				}
				else if (includeUpper && upperPoint > 0)
				{
					inclusiveUpperPoint = upperPoint;
				}
				else if (upperPoint > 0)
				{
					inclusiveUpperPoint = upperPoint - 1;
				}
				else
				{
					inclusiveUpperPoint = - upperPoint - 2;
				}
				
				if (inclusiveUpperPoint <= 0 || inclusiveLowerPoint > inclusiveUpperPoint)
					return DocIdSet.EMPTY_DOCIDSET;
				
				System.Diagnostics.Debug.Assert(inclusiveLowerPoint > 0 && inclusiveUpperPoint > 0);
				
				// for this DocIdSet, we never need to use TermDocs,
				// because deleted docs have an order of 0 (null entry in StringIndex)
				return new AnonymousClassFieldCacheDocIdSet(fcsi, inclusiveLowerPoint, inclusiveUpperPoint, this, reader, false);
			}
		}
		[Serializable]
		private class AnonymousClassFieldCacheRangeFilter1:FieldCacheRangeFilter
		{
			private class AnonymousClassFieldCacheDocIdSet:FieldCacheDocIdSet
			{
				private void  InitBlock(sbyte[] values, byte inclusiveLowerPoint, byte inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter1 enclosingInstance)
				{
					this.values = values;
					this.inclusiveLowerPoint = inclusiveLowerPoint;
					this.inclusiveUpperPoint = inclusiveUpperPoint;
					this.enclosingInstance = enclosingInstance;
				}
				private sbyte[] values;
				private byte inclusiveLowerPoint;
				private byte inclusiveUpperPoint;
				private AnonymousClassFieldCacheRangeFilter1 enclosingInstance;
				public AnonymousClassFieldCacheRangeFilter1 Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				internal AnonymousClassFieldCacheDocIdSet(sbyte[] values, byte inclusiveLowerPoint, byte inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter1 enclosingInstance, Mono.Lucene.Net.Index.IndexReader Param1, bool Param2):base(Param1, Param2)
				{
					InitBlock(values, inclusiveLowerPoint, inclusiveUpperPoint, enclosingInstance);
				}
				internal override bool MatchDoc(int doc)
				{
					return values[doc] >= inclusiveLowerPoint && values[doc] <= inclusiveUpperPoint;
				}
			}
			internal AnonymousClassFieldCacheRangeFilter1(System.String Param1, Mono.Lucene.Net.Search.Parser Param2, System.Object Param3, System.Object Param4, bool Param5, bool Param6):base(Param1, Param2, Param3, Param4, Param5, Param6)
			{
			}
			public override DocIdSet GetDocIdSet(IndexReader reader)
			{
				byte inclusiveLowerPoint;
				byte inclusiveUpperPoint;
				if (lowerVal != null)
				{
					byte i = (byte) System.Convert.ToSByte(((System.ValueType) lowerVal));
					if (!includeLower && i == (byte) System.Byte.MaxValue)
						return DocIdSet.EMPTY_DOCIDSET;
					inclusiveLowerPoint = (byte) (includeLower?i:(i + 1));
				}
				else
				{
					inclusiveLowerPoint = (byte) System.Byte.MinValue;
				}
				if (upperVal != null)
				{
					byte i = (byte) System.Convert.ToSByte(((System.ValueType) upperVal));
					if (!includeUpper && i == (byte) System.Byte.MinValue)
						return DocIdSet.EMPTY_DOCIDSET;
					inclusiveUpperPoint = (byte) (includeUpper?i:(i - 1));
				}
				else
				{
					inclusiveUpperPoint = (byte) System.Byte.MaxValue;
				}
				
				if (inclusiveLowerPoint > inclusiveUpperPoint)
					return DocIdSet.EMPTY_DOCIDSET;
				
				sbyte[] values = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetBytes(reader, field, (Mono.Lucene.Net.Search.ByteParser) parser);
				// we only request the usage of termDocs, if the range contains 0
				return new AnonymousClassFieldCacheDocIdSet(values, inclusiveLowerPoint, inclusiveUpperPoint, this, reader, (inclusiveLowerPoint <= 0 && inclusiveUpperPoint >= 0));
			}
		}
		[Serializable]
		private class AnonymousClassFieldCacheRangeFilter2:FieldCacheRangeFilter
		{
			private class AnonymousClassFieldCacheDocIdSet:FieldCacheDocIdSet
			{
				private void  InitBlock(short[] values, short inclusiveLowerPoint, short inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter2 enclosingInstance)
				{
					this.values = values;
					this.inclusiveLowerPoint = inclusiveLowerPoint;
					this.inclusiveUpperPoint = inclusiveUpperPoint;
					this.enclosingInstance = enclosingInstance;
				}
				private short[] values;
				private short inclusiveLowerPoint;
				private short inclusiveUpperPoint;
				private AnonymousClassFieldCacheRangeFilter2 enclosingInstance;
				public AnonymousClassFieldCacheRangeFilter2 Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				internal AnonymousClassFieldCacheDocIdSet(short[] values, short inclusiveLowerPoint, short inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter2 enclosingInstance, Mono.Lucene.Net.Index.IndexReader Param1, bool Param2):base(Param1, Param2)
				{
					InitBlock(values, inclusiveLowerPoint, inclusiveUpperPoint, enclosingInstance);
				}
				internal override bool MatchDoc(int doc)
				{
					return values[doc] >= inclusiveLowerPoint && values[doc] <= inclusiveUpperPoint;
				}
			}
			internal AnonymousClassFieldCacheRangeFilter2(System.String Param1, Mono.Lucene.Net.Search.Parser Param2, System.Object Param3, System.Object Param4, bool Param5, bool Param6):base(Param1, Param2, Param3, Param4, Param5, Param6)
			{
			}
			public override DocIdSet GetDocIdSet(IndexReader reader)
			{
				short inclusiveLowerPoint;
				short inclusiveUpperPoint;
				if (lowerVal != null)
				{
					short i = System.Convert.ToInt16(((System.ValueType) lowerVal));
					if (!includeLower && i == System.Int16.MaxValue)
						return DocIdSet.EMPTY_DOCIDSET;
					inclusiveLowerPoint = (short) (includeLower?i:(i + 1));
				}
				else
				{
					inclusiveLowerPoint = System.Int16.MinValue;
				}
				if (upperVal != null)
				{
					short i = System.Convert.ToInt16(((System.ValueType) upperVal));
					if (!includeUpper && i == System.Int16.MinValue)
						return DocIdSet.EMPTY_DOCIDSET;
					inclusiveUpperPoint = (short) (includeUpper?i:(i - 1));
				}
				else
				{
					inclusiveUpperPoint = System.Int16.MaxValue;
				}
				
				if (inclusiveLowerPoint > inclusiveUpperPoint)
					return DocIdSet.EMPTY_DOCIDSET;
				
				short[] values = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetShorts(reader, field, (Mono.Lucene.Net.Search.ShortParser) parser);
				// we only request the usage of termDocs, if the range contains 0
				return new AnonymousClassFieldCacheDocIdSet(values, inclusiveLowerPoint, inclusiveUpperPoint, this, reader, (inclusiveLowerPoint <= 0 && inclusiveUpperPoint >= 0));
			}
		}
		[Serializable]
		private class AnonymousClassFieldCacheRangeFilter3:FieldCacheRangeFilter
		{
			private class AnonymousClassFieldCacheDocIdSet:FieldCacheDocIdSet
			{
				private void  InitBlock(int[] values, int inclusiveLowerPoint, int inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter3 enclosingInstance)
				{
					this.values = values;
					this.inclusiveLowerPoint = inclusiveLowerPoint;
					this.inclusiveUpperPoint = inclusiveUpperPoint;
					this.enclosingInstance = enclosingInstance;
				}
				private int[] values;
				private int inclusiveLowerPoint;
				private int inclusiveUpperPoint;
				private AnonymousClassFieldCacheRangeFilter3 enclosingInstance;
				public AnonymousClassFieldCacheRangeFilter3 Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				internal AnonymousClassFieldCacheDocIdSet(int[] values, int inclusiveLowerPoint, int inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter3 enclosingInstance, Mono.Lucene.Net.Index.IndexReader Param1, bool Param2):base(Param1, Param2)
				{
					InitBlock(values, inclusiveLowerPoint, inclusiveUpperPoint, enclosingInstance);
				}
				internal override bool MatchDoc(int doc)
				{
					return values[doc] >= inclusiveLowerPoint && values[doc] <= inclusiveUpperPoint;
				}
			}
			internal AnonymousClassFieldCacheRangeFilter3(System.String Param1, Mono.Lucene.Net.Search.Parser Param2, System.Object Param3, System.Object Param4, bool Param5, bool Param6):base(Param1, Param2, Param3, Param4, Param5, Param6)
			{
			}
			public override DocIdSet GetDocIdSet(IndexReader reader)
			{
				int inclusiveLowerPoint;
				int inclusiveUpperPoint;
				if (lowerVal != null)
				{
					int i = System.Convert.ToInt32(((System.ValueType) lowerVal));
					if (!includeLower && i == System.Int32.MaxValue)
						return DocIdSet.EMPTY_DOCIDSET;
					inclusiveLowerPoint = includeLower?i:(i + 1);
				}
				else
				{
					inclusiveLowerPoint = System.Int32.MinValue;
				}
				if (upperVal != null)
				{
					int i = System.Convert.ToInt32(((System.ValueType) upperVal));
					if (!includeUpper && i == System.Int32.MinValue)
						return DocIdSet.EMPTY_DOCIDSET;
					inclusiveUpperPoint = includeUpper?i:(i - 1);
				}
				else
				{
					inclusiveUpperPoint = System.Int32.MaxValue;
				}
				
				if (inclusiveLowerPoint > inclusiveUpperPoint)
					return DocIdSet.EMPTY_DOCIDSET;
				
				int[] values = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetInts(reader, field, (Mono.Lucene.Net.Search.IntParser) parser);
				// we only request the usage of termDocs, if the range contains 0
				return new AnonymousClassFieldCacheDocIdSet(values, inclusiveLowerPoint, inclusiveUpperPoint, this, reader, (inclusiveLowerPoint <= 0 && inclusiveUpperPoint >= 0));
			}
		}
		[Serializable]
		private class AnonymousClassFieldCacheRangeFilter4:FieldCacheRangeFilter
		{
			private class AnonymousClassFieldCacheDocIdSet:FieldCacheDocIdSet
			{
				private void  InitBlock(long[] values, long inclusiveLowerPoint, long inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter4 enclosingInstance)
				{
					this.values = values;
					this.inclusiveLowerPoint = inclusiveLowerPoint;
					this.inclusiveUpperPoint = inclusiveUpperPoint;
					this.enclosingInstance = enclosingInstance;
				}
				private long[] values;
				private long inclusiveLowerPoint;
				private long inclusiveUpperPoint;
				private AnonymousClassFieldCacheRangeFilter4 enclosingInstance;
				public AnonymousClassFieldCacheRangeFilter4 Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				internal AnonymousClassFieldCacheDocIdSet(long[] values, long inclusiveLowerPoint, long inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter4 enclosingInstance, Mono.Lucene.Net.Index.IndexReader Param1, bool Param2):base(Param1, Param2)
				{
					InitBlock(values, inclusiveLowerPoint, inclusiveUpperPoint, enclosingInstance);
				}
				internal override bool MatchDoc(int doc)
				{
					return values[doc] >= inclusiveLowerPoint && values[doc] <= inclusiveUpperPoint;
				}
			}
			internal AnonymousClassFieldCacheRangeFilter4(System.String Param1, Mono.Lucene.Net.Search.Parser Param2, System.Object Param3, System.Object Param4, bool Param5, bool Param6):base(Param1, Param2, Param3, Param4, Param5, Param6)
			{
			}
			public override DocIdSet GetDocIdSet(IndexReader reader)
			{
				long inclusiveLowerPoint;
				long inclusiveUpperPoint;
				if (lowerVal != null)
				{
					long i = System.Convert.ToInt64(((System.ValueType) lowerVal));
					if (!includeLower && i == System.Int64.MaxValue)
						return DocIdSet.EMPTY_DOCIDSET;
					inclusiveLowerPoint = includeLower?i:(i + 1L);
				}
				else
				{
					inclusiveLowerPoint = System.Int64.MinValue;
				}
				if (upperVal != null)
				{
					long i = System.Convert.ToInt64(((System.ValueType) upperVal));
					if (!includeUpper && i == System.Int64.MinValue)
						return DocIdSet.EMPTY_DOCIDSET;
					inclusiveUpperPoint = includeUpper?i:(i - 1L);
				}
				else
				{
					inclusiveUpperPoint = System.Int64.MaxValue;
				}
				
				if (inclusiveLowerPoint > inclusiveUpperPoint)
					return DocIdSet.EMPTY_DOCIDSET;
				
				long[] values = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetLongs(reader, field, (Mono.Lucene.Net.Search.LongParser) parser);
				// we only request the usage of termDocs, if the range contains 0
				return new AnonymousClassFieldCacheDocIdSet(values, inclusiveLowerPoint, inclusiveUpperPoint, this, reader, (inclusiveLowerPoint <= 0L && inclusiveUpperPoint >= 0L));
			}
		}
		[Serializable]
		private class AnonymousClassFieldCacheRangeFilter5:FieldCacheRangeFilter
		{
			private class AnonymousClassFieldCacheDocIdSet:FieldCacheDocIdSet
			{
				private void  InitBlock(float[] values, float inclusiveLowerPoint, float inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter5 enclosingInstance)
				{
					this.values = values;
					this.inclusiveLowerPoint = inclusiveLowerPoint;
					this.inclusiveUpperPoint = inclusiveUpperPoint;
					this.enclosingInstance = enclosingInstance;
				}
				private float[] values;
				private float inclusiveLowerPoint;
				private float inclusiveUpperPoint;
				private AnonymousClassFieldCacheRangeFilter5 enclosingInstance;
				public AnonymousClassFieldCacheRangeFilter5 Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				internal AnonymousClassFieldCacheDocIdSet(float[] values, float inclusiveLowerPoint, float inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter5 enclosingInstance, Mono.Lucene.Net.Index.IndexReader Param1, bool Param2):base(Param1, Param2)
				{
					InitBlock(values, inclusiveLowerPoint, inclusiveUpperPoint, enclosingInstance);
				}
				internal override bool MatchDoc(int doc)
				{
					return values[doc] >= inclusiveLowerPoint && values[doc] <= inclusiveUpperPoint;
				}
			}
			internal AnonymousClassFieldCacheRangeFilter5(System.String Param1, Mono.Lucene.Net.Search.Parser Param2, System.Object Param3, System.Object Param4, bool Param5, bool Param6):base(Param1, Param2, Param3, Param4, Param5, Param6)
			{
			}
			public override DocIdSet GetDocIdSet(IndexReader reader)
			{
				// we transform the floating point numbers to sortable integers
				// using NumericUtils to easier find the next bigger/lower value
				float inclusiveLowerPoint;
				float inclusiveUpperPoint;
				if (lowerVal != null)
				{
					float f = System.Convert.ToSingle(((System.ValueType) lowerVal));
					if (!includeUpper && f > 0.0f && System.Single.IsInfinity(f))
						return DocIdSet.EMPTY_DOCIDSET;
					int i = NumericUtils.FloatToSortableInt(f);
					inclusiveLowerPoint = NumericUtils.SortableIntToFloat(includeLower?i:(i + 1));
				}
				else
				{
					inclusiveLowerPoint = System.Single.NegativeInfinity;
				}
				if (upperVal != null)
				{
					float f = System.Convert.ToSingle(((System.ValueType) upperVal));
					if (!includeUpper && f < 0.0f && System.Single.IsInfinity(f))
						return DocIdSet.EMPTY_DOCIDSET;
					int i = NumericUtils.FloatToSortableInt(f);
					inclusiveUpperPoint = NumericUtils.SortableIntToFloat(includeUpper?i:(i - 1));
				}
				else
				{
					inclusiveUpperPoint = System.Single.PositiveInfinity;
				}
				
				if (inclusiveLowerPoint > inclusiveUpperPoint)
					return DocIdSet.EMPTY_DOCIDSET;
				
				float[] values = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetFloats(reader, field, (Mono.Lucene.Net.Search.FloatParser) parser);
				// we only request the usage of termDocs, if the range contains 0
				return new AnonymousClassFieldCacheDocIdSet(values, inclusiveLowerPoint, inclusiveUpperPoint, this, reader, (inclusiveLowerPoint <= 0.0f && inclusiveUpperPoint >= 0.0f));
			}
		}
		[Serializable]
		private class AnonymousClassFieldCacheRangeFilter6:FieldCacheRangeFilter
		{
			private class AnonymousClassFieldCacheDocIdSet:FieldCacheDocIdSet
			{
				private void  InitBlock(double[] values, double inclusiveLowerPoint, double inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter6 enclosingInstance)
				{
					this.values = values;
					this.inclusiveLowerPoint = inclusiveLowerPoint;
					this.inclusiveUpperPoint = inclusiveUpperPoint;
					this.enclosingInstance = enclosingInstance;
				}
				private double[] values;
				private double inclusiveLowerPoint;
				private double inclusiveUpperPoint;
				private AnonymousClassFieldCacheRangeFilter6 enclosingInstance;
				public AnonymousClassFieldCacheRangeFilter6 Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				internal AnonymousClassFieldCacheDocIdSet(double[] values, double inclusiveLowerPoint, double inclusiveUpperPoint, AnonymousClassFieldCacheRangeFilter6 enclosingInstance, Mono.Lucene.Net.Index.IndexReader Param1, bool Param2):base(Param1, Param2)
				{
					InitBlock(values, inclusiveLowerPoint, inclusiveUpperPoint, enclosingInstance);
				}
				internal override bool MatchDoc(int doc)
				{
					return values[doc] >= inclusiveLowerPoint && values[doc] <= inclusiveUpperPoint;
				}
			}
			internal AnonymousClassFieldCacheRangeFilter6(System.String Param1, Mono.Lucene.Net.Search.Parser Param2, System.Object Param3, System.Object Param4, bool Param5, bool Param6):base(Param1, Param2, Param3, Param4, Param5, Param6)
			{
			}
			public override DocIdSet GetDocIdSet(IndexReader reader)
			{
				// we transform the floating point numbers to sortable integers
				// using NumericUtils to easier find the next bigger/lower value
				double inclusiveLowerPoint;
				double inclusiveUpperPoint;
				if (lowerVal != null)
				{
					double f = System.Convert.ToDouble(((System.ValueType) lowerVal));
					if (!includeUpper && f > 0.0 && System.Double.IsInfinity(f))
						return DocIdSet.EMPTY_DOCIDSET;
					long i = NumericUtils.DoubleToSortableLong(f);
					inclusiveLowerPoint = NumericUtils.SortableLongToDouble(includeLower?i:(i + 1L));
				}
				else
				{
					inclusiveLowerPoint = System.Double.NegativeInfinity;
				}
				if (upperVal != null)
				{
					double f = System.Convert.ToDouble(((System.ValueType) upperVal));
					if (!includeUpper && f < 0.0 && System.Double.IsInfinity(f))
						return DocIdSet.EMPTY_DOCIDSET;
					long i = NumericUtils.DoubleToSortableLong(f);
					inclusiveUpperPoint = NumericUtils.SortableLongToDouble(includeUpper?i:(i - 1L));
				}
				else
				{
					inclusiveUpperPoint = System.Double.PositiveInfinity;
				}
				
				if (inclusiveLowerPoint > inclusiveUpperPoint)
					return DocIdSet.EMPTY_DOCIDSET;
				
				double[] values = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetDoubles(reader, field, (Mono.Lucene.Net.Search.DoubleParser) parser);
				// we only request the usage of termDocs, if the range contains 0
				return new AnonymousClassFieldCacheDocIdSet(values, inclusiveLowerPoint, inclusiveUpperPoint, this, reader, (inclusiveLowerPoint <= 0.0 && inclusiveUpperPoint >= 0.0));
			}
		}
		internal System.String field;
		internal Mono.Lucene.Net.Search.Parser parser;
		internal System.Object lowerVal;
		internal System.Object upperVal;
		internal bool includeLower;
		internal bool includeUpper;
		
		private FieldCacheRangeFilter(System.String field, Mono.Lucene.Net.Search.Parser parser, System.Object lowerVal, System.Object upperVal, bool includeLower, bool includeUpper)
		{
			this.field = field;
			this.parser = parser;
			this.lowerVal = lowerVal;
			this.upperVal = upperVal;
			this.includeLower = includeLower;
			this.includeUpper = includeUpper;
		}
		
		/// <summary>This method is implemented for each data type </summary>
		public abstract override DocIdSet GetDocIdSet(IndexReader reader);
		
		/// <summary> Creates a string range query using {@link FieldCache#getStringIndex}. This works with all
		/// fields containing zero or one term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewStringRange(System.String field, System.String lowerVal, System.String upperVal, bool includeLower, bool includeUpper)
		{
			return new AnonymousClassFieldCacheRangeFilter(field, null, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetBytes(IndexReader,String)}. This works with all
		/// byte fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewByteRange(System.String field, System.Byte lowerVal, System.Byte upperVal, bool includeLower, bool includeUpper)
		{
			return NewByteRange(field, null, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetBytes(IndexReader,String,FieldCache.ByteParser)}. This works with all
		/// byte fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewByteRange(System.String field, Mono.Lucene.Net.Search.ByteParser parser, System.Byte lowerVal, System.Byte upperVal, bool includeLower, bool includeUpper)
		{
			return new AnonymousClassFieldCacheRangeFilter1(field, parser, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetShorts(IndexReader,String)}. This works with all
		/// short fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewShortRange(System.String field, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return NewShortRange(field, null, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetShorts(IndexReader,String,FieldCache.ShortParser)}. This works with all
		/// short fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewShortRange(System.String field, Mono.Lucene.Net.Search.ShortParser parser, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return new AnonymousClassFieldCacheRangeFilter2(field, parser, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetInts(IndexReader,String)}. This works with all
		/// int fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
        public static FieldCacheRangeFilter NewIntRange(System.String field, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return NewIntRange(field, null, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetInts(IndexReader,String,FieldCache.IntParser)}. This works with all
		/// int fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewIntRange(System.String field, Mono.Lucene.Net.Search.IntParser parser, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return new AnonymousClassFieldCacheRangeFilter3(field, parser, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetLongs(IndexReader,String)}. This works with all
		/// long fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewLongRange(System.String field, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return NewLongRange(field, null, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetLongs(IndexReader,String,FieldCache.LongParser)}. This works with all
		/// long fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewLongRange(System.String field, Mono.Lucene.Net.Search.LongParser parser, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return new AnonymousClassFieldCacheRangeFilter4(field, parser, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetFloats(IndexReader,String)}. This works with all
		/// float fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewFloatRange(System.String field, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return NewFloatRange(field, null, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetFloats(IndexReader,String,FieldCache.FloatParser)}. This works with all
		/// float fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewFloatRange(System.String field, Mono.Lucene.Net.Search.FloatParser parser, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return new AnonymousClassFieldCacheRangeFilter5(field, parser, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetDoubles(IndexReader,String)}. This works with all
		/// double fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewDoubleRange(System.String field, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return NewDoubleRange(field, null, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		/// <summary> Creates a numeric range query using {@link FieldCache#GetDoubles(IndexReader,String,FieldCache.DoubleParser)}. This works with all
		/// double fields containing exactly one numeric term in the field. The range can be half-open by setting one
		/// of the values to <code>null</code>.
		/// </summary>
		public static FieldCacheRangeFilter NewDoubleRange(System.String field, Mono.Lucene.Net.Search.DoubleParser parser, System.ValueType lowerVal, System.ValueType upperVal, bool includeLower, bool includeUpper)
		{
			return new AnonymousClassFieldCacheRangeFilter6(field, parser, lowerVal, upperVal, includeLower, includeUpper);
		}
		
		public override System.String ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(field).Append(":");
			return sb.Append(includeLower?'[':'{').Append((lowerVal == null)?"*":lowerVal.ToString()).Append(" TO ").Append((upperVal == null)?"*":upperVal.ToString()).Append(includeUpper?']':'}').ToString();
		}
		
		public  override bool Equals(System.Object o)
		{
			if (this == o)
				return true;
			if (!(o is FieldCacheRangeFilter))
				return false;
			FieldCacheRangeFilter other = (FieldCacheRangeFilter) o;
			
			if (!this.field.Equals(other.field) || this.includeLower != other.includeLower || this.includeUpper != other.includeUpper)
			{
				return false;
			}
			if (this.lowerVal != null?!this.lowerVal.Equals(other.lowerVal):other.lowerVal != null)
				return false;
			if (this.upperVal != null?!this.upperVal.Equals(other.upperVal):other.upperVal != null)
				return false;
			if (this.parser != null?!this.parser.Equals(other.parser):other.parser != null)
				return false;
			return true;
		}
		
		public override int GetHashCode()
		{
			int h = field.GetHashCode();
			h ^= ((lowerVal != null)?lowerVal.GetHashCode():550356204);
			h = (h << 1) | (SupportClass.Number.URShift(h, 31)); // rotate to distinguish lower from upper
			h ^= ((upperVal != null)?upperVal.GetHashCode():- 1674416163);
			h ^= ((parser != null)?parser.GetHashCode():- 1572457324);
			h ^= (includeLower?1549299360:- 365038026) ^ (includeUpper?1721088258:1948649653);
			return h;
		}
		
		internal abstract class FieldCacheDocIdSet:DocIdSet
		{
			private class AnonymousClassDocIdSetIterator:DocIdSetIterator
			{
				public AnonymousClassDocIdSetIterator(Mono.Lucene.Net.Index.TermDocs termDocs, FieldCacheDocIdSet enclosingInstance)
				{
					InitBlock(termDocs, enclosingInstance);
				}
				private void  InitBlock(Mono.Lucene.Net.Index.TermDocs termDocs, FieldCacheDocIdSet enclosingInstance)
				{
					this.termDocs = termDocs;
					this.enclosingInstance = enclosingInstance;
				}
				private Mono.Lucene.Net.Index.TermDocs termDocs;
				private FieldCacheDocIdSet enclosingInstance;
				public FieldCacheDocIdSet Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				private int doc = - 1;
				
				/** @deprecated use {@link #NextDoc()} instead. */
                [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Search.DocIdSetIterator.Next()")]
				public override bool Next()
				{
					return NextDoc() != NO_MORE_DOCS;
				}
				
				/// <deprecated> use {@link #Advance(int)} instead. 
				/// </deprecated>
                [Obsolete("use Advance(int) instead.")]
				public override bool SkipTo(int target)
				{
					return Advance(target) != NO_MORE_DOCS;
				}
				
				/// <deprecated> use {@link #DocID()} instead. 
				/// </deprecated>
                [Obsolete("use DocID() instead.")]
				public override int Doc()
				{
					return termDocs.Doc();
				}
				
				public override int DocID()
				{
					return doc;
				}
				
				public override int NextDoc()
				{
					do 
					{
						if (!termDocs.Next())
							return doc = NO_MORE_DOCS;
					}
					while (!Enclosing_Instance.MatchDoc(doc = termDocs.Doc()));
					return doc;
				}
				
				public override int Advance(int target)
				{
					if (!termDocs.SkipTo(target))
						return doc = NO_MORE_DOCS;
					while (!Enclosing_Instance.MatchDoc(doc = termDocs.Doc()))
					{
						if (!termDocs.Next())
							return doc = NO_MORE_DOCS;
					}
					return doc;
				}
			}
			private class AnonymousClassDocIdSetIterator1:DocIdSetIterator
			{
				public AnonymousClassDocIdSetIterator1(FieldCacheDocIdSet enclosingInstance)
				{
					InitBlock(enclosingInstance);
				}
				private void  InitBlock(FieldCacheDocIdSet enclosingInstance)
				{
					this.enclosingInstance = enclosingInstance;
				}
				private FieldCacheDocIdSet enclosingInstance;
				public FieldCacheDocIdSet Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				private int doc = - 1;
				
				/// <deprecated> use {@link #NextDoc()} instead. 
				/// </deprecated>
                [Obsolete("use NextDoc() instead.")]
				public override bool Next()
				{
					return NextDoc() != NO_MORE_DOCS;
				}
				
				/// <deprecated> use {@link #Advance(int)} instead. 
				/// </deprecated>
                [Obsolete("use Advance(int) instead.")]
				public override bool SkipTo(int target)
				{
					return Advance(target) != NO_MORE_DOCS;
				}
				
				/// <deprecated> use {@link #DocID()} instead. 
				/// </deprecated>
                [Obsolete("use DocID() instead. ")]
				public override int Doc()
				{
					return doc;
				}
				
				public override int DocID()
				{
					return doc;
				}
				
				public override int NextDoc()
				{
					try
					{
						do 
						{
							doc++;
						}
						while (!Enclosing_Instance.MatchDoc(doc));
						return doc;
					}
					catch (System.IndexOutOfRangeException e)
					{
						return doc = NO_MORE_DOCS;
					}
				}
				
				public override int Advance(int target)
				{
					try
					{
						doc = target;
						while (!Enclosing_Instance.MatchDoc(doc))
						{
							doc++;
						}
						return doc;
					}
					catch (System.IndexOutOfRangeException e)
					{
						return doc = NO_MORE_DOCS;
					}
				}
			}
			private IndexReader reader;
			private bool mayUseTermDocs;
			
			internal FieldCacheDocIdSet(IndexReader reader, bool mayUseTermDocs)
			{
				this.reader = reader;
				this.mayUseTermDocs = mayUseTermDocs;
			}
			
			/// <summary>this method checks, if a doc is a hit, should throw AIOBE, when position invalid </summary>
			internal abstract bool MatchDoc(int doc);

			/// <summary>this DocIdSet is cacheable, if it works solely with FieldCache and no TermDocs </summary>
			public override bool IsCacheable()
			{
				return !(mayUseTermDocs && reader.HasDeletions());
			}
			
			public override DocIdSetIterator Iterator()
			{
				// Synchronization needed because deleted docs BitVector
				// can change after call to hasDeletions until TermDocs creation.
				// We only use an iterator with termDocs, when this was requested (e.g. range contains 0)
				// and the index has deletions
				TermDocs termDocs;
				lock (reader)
				{
					termDocs = IsCacheable() ? null : reader.TermDocs(null);
				}
				if (termDocs != null)
				{
					// a DocIdSetIterator using TermDocs to iterate valid docIds
					return new AnonymousClassDocIdSetIterator(termDocs, this);
				}
				else
				{
					// a DocIdSetIterator generating docIds by incrementing a variable -
					// this one can be used if there are no deletions are on the index
					return new AnonymousClassDocIdSetIterator1(this);
				}
			}
		}
	}
}
