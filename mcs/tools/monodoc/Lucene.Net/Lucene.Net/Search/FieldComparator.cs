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

using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using ByteParser = Mono.Lucene.Net.Search.ByteParser;
using DoubleParser = Mono.Lucene.Net.Search.DoubleParser;
using FloatParser = Mono.Lucene.Net.Search.FloatParser;
using IntParser = Mono.Lucene.Net.Search.IntParser;
using LongParser = Mono.Lucene.Net.Search.LongParser;
using ShortParser = Mono.Lucene.Net.Search.ShortParser;
using StringIndex = Mono.Lucene.Net.Search.StringIndex;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Expert: a FieldComparator compares hits so as to determine their
	/// sort order when collecting the top results with {@link
	/// TopFieldCollector}.  The concrete public FieldComparator
	/// classes here correspond to the SortField types.
	/// 
	/// <p/>This API is designed to achieve high performance
	/// sorting, by exposing a tight interaction with {@link
	/// FieldValueHitQueue} as it visits hits.  Whenever a hit is
	/// competitive, it's enrolled into a virtual slot, which is
	/// an int ranging from 0 to numHits-1.  The {@link
	/// FieldComparator} is made aware of segment transitions
	/// during searching in case any internal state it's tracking
	/// needs to be recomputed during these transitions.<p/>
	/// 
	/// <p/>A comparator must define these functions:<p/>
	/// 
	/// <ul>
	/// 
	/// <li> {@link #compare} Compare a hit at 'slot a'
	/// with hit 'slot b'.</li>
	/// 
	/// <li> {@link #setBottom} This method is called by
	/// {@link FieldValueHitQueue} to notify the
	/// FieldComparator of the current weakest ("bottom")
	/// slot.  Note that this slot may not hold the weakest
	/// value according to your comparator, in cases where
	/// your comparator is not the primary one (ie, is only
	/// used to break ties from the comparators before it).</li>
	/// 
	/// <li> {@link #compareBottom} Compare a new hit (docID)
	/// against the "weakest" (bottom) entry in the queue.</li>
	/// 
	/// <li> {@link #copy} Installs a new hit into the
	/// priority queue.  The {@link FieldValueHitQueue}
	/// calls this method when a new hit is competitive.</li>
	/// 
	/// <li> {@link #setNextReader} Invoked
	/// when the search is switching to the next segment.
	/// You may need to update internal state of the
	/// comparator, for example retrieving new values from
	/// the {@link FieldCache}.</li>
	/// 
	/// <li> {@link #value} Return the sort value stored in
	/// the specified slot.  This is only called at the end
	/// of the search, in order to populate {@link
	/// FieldDoc#fields} when returning the top results.</li>
	/// </ul>
	/// 
	/// <b>NOTE:</b> This API is experimental and might change in
	/// incompatible ways in the next release.
	/// </summary>
	public abstract class FieldComparator
	{
		
		/// <summary>Parses field's values as byte (using {@link
		/// FieldCache#getBytes} and sorts by ascending value 
		/// </summary>
		public sealed class ByteComparator:FieldComparator
		{
			private sbyte[] values;
			private sbyte[] currentReaderValues;
			private System.String field;
			private ByteParser parser;
			private sbyte bottom;
			
			internal ByteComparator(int numHits, System.String field, Mono.Lucene.Net.Search.Parser parser)
			{
				values = new sbyte[numHits];
				this.field = field;
				this.parser = (ByteParser) parser;
			}
			
			public override int Compare(int slot1, int slot2)
			{
				return values[slot1] - values[slot2];
			}
			
			public override int CompareBottom(int doc)
			{
				return bottom - currentReaderValues[doc];
			}
			
			public override void  Copy(int slot, int doc)
			{
				values[slot] = currentReaderValues[doc];
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				currentReaderValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetBytes(reader, field, parser);
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = values[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return (sbyte) values[slot];
			}
		}
		
		/// <summary>Sorts by ascending docID </summary>
		public sealed class DocComparator:FieldComparator
		{
			private int[] docIDs;
			private int docBase;
			private int bottom;
			
			internal DocComparator(int numHits)
			{
				docIDs = new int[numHits];
			}
			
			public override int Compare(int slot1, int slot2)
			{
				// No overflow risk because docIDs are non-negative
				return docIDs[slot1] - docIDs[slot2];
			}
			
			public override int CompareBottom(int doc)
			{
				// No overflow risk because docIDs are non-negative
				return bottom - (docBase + doc);
			}
			
			public override void  Copy(int slot, int doc)
			{
				docIDs[slot] = docBase + doc;
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				// TODO: can we "map" our docIDs to the current
				// reader? saves having to then subtract on every
				// compare call
				this.docBase = docBase;
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = docIDs[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return (System.Int32) docIDs[slot];
			}
		}
		
		/// <summary>Parses field's values as double (using {@link
		/// FieldCache#getDoubles} and sorts by ascending value 
		/// </summary>
		public sealed class DoubleComparator:FieldComparator
		{
			private double[] values;
			private double[] currentReaderValues;
			private System.String field;
			private DoubleParser parser;
			private double bottom;
			
			internal DoubleComparator(int numHits, System.String field, Mono.Lucene.Net.Search.Parser parser)
			{
				values = new double[numHits];
				this.field = field;
				this.parser = (DoubleParser) parser;
			}
			
			public override int Compare(int slot1, int slot2)
			{
				double v1 = values[slot1];
				double v2 = values[slot2];
				if (v1 > v2)
				{
					return 1;
				}
				else if (v1 < v2)
				{
					return - 1;
				}
				else
				{
					return 0;
				}
			}
			
			public override int CompareBottom(int doc)
			{
				double v2 = currentReaderValues[doc];
				if (bottom > v2)
				{
					return 1;
				}
				else if (bottom < v2)
				{
					return - 1;
				}
				else
				{
					return 0;
				}
			}
			
			public override void  Copy(int slot, int doc)
			{
				values[slot] = currentReaderValues[doc];
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				currentReaderValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetDoubles(reader, field, parser);
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = values[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return (double) values[slot];
			}
		}
		
		/// <summary>Parses field's values as float (using {@link
		/// FieldCache#getFloats} and sorts by ascending value 
		/// </summary>
		public sealed class FloatComparator:FieldComparator
		{
			private float[] values;
			private float[] currentReaderValues;
			private System.String field;
			private FloatParser parser;
			private float bottom;
			
			internal FloatComparator(int numHits, System.String field, Mono.Lucene.Net.Search.Parser parser)
			{
				values = new float[numHits];
				this.field = field;
				this.parser = (FloatParser) parser;
			}
			
			public override int Compare(int slot1, int slot2)
			{
				// TODO: are there sneaky non-branch ways to compute
				// sign of float?
				float v1 = values[slot1];
				float v2 = values[slot2];
				if (v1 > v2)
				{
					return 1;
				}
				else if (v1 < v2)
				{
					return - 1;
				}
				else
				{
					return 0;
				}
			}
			
			public override int CompareBottom(int doc)
			{
				// TODO: are there sneaky non-branch ways to compute
				// sign of float?
				float v2 = currentReaderValues[doc];
				if (bottom > v2)
				{
					return 1;
				}
				else if (bottom < v2)
				{
					return - 1;
				}
				else
				{
					return 0;
				}
			}
			
			public override void  Copy(int slot, int doc)
			{
				values[slot] = currentReaderValues[doc];
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				currentReaderValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetFloats(reader, field, parser);
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = values[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return (float) values[slot];
			}
		}
		
		/// <summary>Parses field's values as int (using {@link
		/// FieldCache#getInts} and sorts by ascending value 
		/// </summary>
		public sealed class IntComparator:FieldComparator
		{
			private int[] values;
			private int[] currentReaderValues;
			private System.String field;
			private IntParser parser;
			private int bottom; // Value of bottom of queue
			
			internal IntComparator(int numHits, System.String field, Mono.Lucene.Net.Search.Parser parser)
			{
				values = new int[numHits];
				this.field = field;
				this.parser = (IntParser) parser;
			}
			
			public override int Compare(int slot1, int slot2)
			{
				// TODO: there are sneaky non-branch ways to compute
				// -1/+1/0 sign
				// Cannot return values[slot1] - values[slot2] because that
				// may overflow
				int v1 = values[slot1];
				int v2 = values[slot2];
				if (v1 > v2)
				{
					return 1;
				}
				else if (v1 < v2)
				{
					return - 1;
				}
				else
				{
					return 0;
				}
			}
			
			public override int CompareBottom(int doc)
			{
				// TODO: there are sneaky non-branch ways to compute
				// -1/+1/0 sign
				// Cannot return bottom - values[slot2] because that
				// may overflow
				int v2 = currentReaderValues[doc];
				if (bottom > v2)
				{
					return 1;
				}
				else if (bottom < v2)
				{
					return - 1;
				}
				else
				{
					return 0;
				}
			}
			
			public override void  Copy(int slot, int doc)
			{
				values[slot] = currentReaderValues[doc];
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				currentReaderValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetInts(reader, field, parser);
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = values[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return (System.Int32) values[slot];
			}
		}
		
		/// <summary>Parses field's values as long (using {@link
		/// FieldCache#getLongs} and sorts by ascending value 
		/// </summary>
		public sealed class LongComparator:FieldComparator
		{
			private long[] values;
			private long[] currentReaderValues;
			private System.String field;
			private LongParser parser;
			private long bottom;
			
			internal LongComparator(int numHits, System.String field, Mono.Lucene.Net.Search.Parser parser)
			{
				values = new long[numHits];
				this.field = field;
				this.parser = (LongParser) parser;
			}
			
			public override int Compare(int slot1, int slot2)
			{
				// TODO: there are sneaky non-branch ways to compute
				// -1/+1/0 sign
				long v1 = values[slot1];
				long v2 = values[slot2];
				if (v1 > v2)
				{
					return 1;
				}
				else if (v1 < v2)
				{
					return - 1;
				}
				else
				{
					return 0;
				}
			}
			
			public override int CompareBottom(int doc)
			{
				// TODO: there are sneaky non-branch ways to compute
				// -1/+1/0 sign
				long v2 = currentReaderValues[doc];
				if (bottom > v2)
				{
					return 1;
				}
				else if (bottom < v2)
				{
					return - 1;
				}
				else
				{
					return 0;
				}
			}
			
			public override void  Copy(int slot, int doc)
			{
				values[slot] = currentReaderValues[doc];
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				currentReaderValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetLongs(reader, field, parser);
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = values[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return (long) values[slot];
			}
		}
		
		/// <summary>Sorts by descending relevance.  NOTE: if you are
		/// sorting only by descending relevance and then
		/// secondarily by ascending docID, peformance is faster
		/// using {@link TopScoreDocCollector} directly (which {@link
		/// IndexSearcher#search} uses when no {@link Sort} is
		/// specified). 
		/// </summary>
		public sealed class RelevanceComparator:FieldComparator
		{
			private float[] scores;
			private float bottom;
			private Scorer scorer;
			
			internal RelevanceComparator(int numHits)
			{
				scores = new float[numHits];
			}
			
			public override int Compare(int slot1, int slot2)
			{
				float score1 = scores[slot1];
				float score2 = scores[slot2];
				return score1 > score2?- 1:(score1 < score2?1:0);
			}
			
			public override int CompareBottom(int doc)
			{
				float score = scorer.Score();
				return bottom > score?- 1:(bottom < score?1:0);
			}
			
			public override void  Copy(int slot, int doc)
			{
				scores[slot] = scorer.Score();
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = scores[bottom];
			}
			
			public override void  SetScorer(Scorer scorer)
			{
				// wrap with a ScoreCachingWrappingScorer so that successive calls to
				// score() will not incur score computation over and over again.
				this.scorer = new ScoreCachingWrappingScorer(scorer);
			}
			
			public override System.IComparable Value(int slot)
			{
				return (float) scores[slot];
			}
		}
		
		/// <summary>Parses field's values as short (using {@link
		/// FieldCache#getShorts} and sorts by ascending value 
		/// </summary>
		public sealed class ShortComparator:FieldComparator
		{
			private short[] values;
			private short[] currentReaderValues;
			private System.String field;
			private ShortParser parser;
			private short bottom;
			
			internal ShortComparator(int numHits, System.String field, Mono.Lucene.Net.Search.Parser parser)
			{
				values = new short[numHits];
				this.field = field;
				this.parser = (ShortParser) parser;
			}
			
			public override int Compare(int slot1, int slot2)
			{
				return values[slot1] - values[slot2];
			}
			
			public override int CompareBottom(int doc)
			{
				return bottom - currentReaderValues[doc];
			}
			
			public override void  Copy(int slot, int doc)
			{
				values[slot] = currentReaderValues[doc];
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				currentReaderValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetShorts(reader, field, parser);
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = values[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return (short) values[slot];
			}
		}
		
		/// <summary>Sorts by a field's value using the Collator for a
		/// given Locale.
		/// </summary>
		public sealed class StringComparatorLocale:FieldComparator
		{
			
			private System.String[] values;
			private System.String[] currentReaderValues;
			private System.String field;
			internal System.Globalization.CompareInfo collator;
			private System.String bottom;
			
			internal StringComparatorLocale(int numHits, System.String field, System.Globalization.CultureInfo locale)
			{
				values = new System.String[numHits];
				this.field = field;
				collator = locale.CompareInfo;
			}
			
			public override int Compare(int slot1, int slot2)
			{
				System.String val1 = values[slot1];
				System.String val2 = values[slot2];
				if (val1 == null)
				{
					if (val2 == null)
					{
						return 0;
					}
					return - 1;
				}
				else if (val2 == null)
				{
					return 1;
				}
				return collator.Compare(val1.ToString(), val2.ToString());
			}
			
			public override int CompareBottom(int doc)
			{
				System.String val2 = currentReaderValues[doc];
				if (bottom == null)
				{
					if (val2 == null)
					{
						return 0;
					}
					return - 1;
				}
				else if (val2 == null)
				{
					return 1;
				}
				return collator.Compare(bottom.ToString(), val2.ToString());
			}
			
			public override void  Copy(int slot, int doc)
			{
				values[slot] = currentReaderValues[doc];
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				currentReaderValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetStrings(reader, field);
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = values[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return values[slot];
			}
		}
		
		/// <summary>Sorts by field's natural String sort order, using
		/// ordinals.  This is functionally equivalent to {@link
		/// StringValComparator}, but it first resolves the string
		/// to their relative ordinal positions (using the index
		/// returned by {@link FieldCache#getStringIndex}), and
		/// does most comparisons using the ordinals.  For medium
		/// to large results, this comparator will be much faster
		/// than {@link StringValComparator}.  For very small
		/// result sets it may be slower. 
		/// </summary>
		public sealed class StringOrdValComparator:FieldComparator
		{
			
			private int[] ords;
			private System.String[] values;
			private int[] readerGen;
			
			private int currentReaderGen = - 1;
			private System.String[] lookup;
			private int[] order;
			private System.String field;
			
			private int bottomSlot = - 1;
			private int bottomOrd;
			private System.String bottomValue;
			private bool reversed;
			private int sortPos;
			
			public StringOrdValComparator(int numHits, System.String field, int sortPos, bool reversed)
			{
				ords = new int[numHits];
				values = new System.String[numHits];
				readerGen = new int[numHits];
				this.sortPos = sortPos;
				this.reversed = reversed;
				this.field = field;
			}
			
			public override int Compare(int slot1, int slot2)
			{
				if (readerGen[slot1] == readerGen[slot2])
				{
					int cmp = ords[slot1] - ords[slot2];
					if (cmp != 0)
					{
						return cmp;
					}
				}
				
				System.String val1 = values[slot1];
				System.String val2 = values[slot2];
				if (val1 == null)
				{
					if (val2 == null)
					{
						return 0;
					}
					return - 1;
				}
				else if (val2 == null)
				{
					return 1;
				}
				return String.CompareOrdinal(val1, val2);
			}
			
			public override int CompareBottom(int doc)
			{
				System.Diagnostics.Debug.Assert(bottomSlot != - 1);
				int order = this.order[doc];
				int cmp = bottomOrd - order;
				if (cmp != 0)
				{
					return cmp;
				}
				
				System.String val2 = lookup[order];
				if (bottomValue == null)
				{
					if (val2 == null)
					{
						return 0;
					}
					// bottom wins
					return - 1;
				}
				else if (val2 == null)
				{
					// doc wins
					return 1;
				}
				return String.CompareOrdinal(bottomValue, val2);
			}
			
			private void  Convert(int slot)
			{
				readerGen[slot] = currentReaderGen;
				int index = 0;
				System.String value_Renamed = values[slot];
				if (value_Renamed == null)
				{
					ords[slot] = 0;
					return ;
				}
				
				if (sortPos == 0 && bottomSlot != - 1 && bottomSlot != slot)
				{
					// Since we are the primary sort, the entries in the
					// queue are bounded by bottomOrd:
					System.Diagnostics.Debug.Assert(bottomOrd < lookup.Length);
					if (reversed)
					{
						index = BinarySearch(lookup, value_Renamed, bottomOrd, lookup.Length - 1);
					}
					else
					{
						index = BinarySearch(lookup, value_Renamed, 0, bottomOrd);
					}
				}
				else
				{
					// Full binary search
					index = BinarySearch(lookup, value_Renamed);
				}
				
				if (index < 0)
				{
					index = - index - 2;
				}
				ords[slot] = index;
			}
			
			public override void  Copy(int slot, int doc)
			{
				int ord = order[doc];
				ords[slot] = ord;
				System.Diagnostics.Debug.Assert(ord >= 0);
				values[slot] = lookup[ord];
				readerGen[slot] = currentReaderGen;
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				StringIndex currentReaderValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetStringIndex(reader, field);
				currentReaderGen++;
				order = currentReaderValues.order;
				lookup = currentReaderValues.lookup;
				System.Diagnostics.Debug.Assert(lookup.Length > 0);
				if (bottomSlot != - 1)
				{
					Convert(bottomSlot);
					bottomOrd = ords[bottomSlot];
				}
			}
			
			public override void  SetBottom(int bottom)
			{
				bottomSlot = bottom;
				if (readerGen[bottom] != currentReaderGen)
				{
					Convert(bottomSlot);
				}
				bottomOrd = ords[bottom];
				System.Diagnostics.Debug.Assert(bottomOrd >= 0);
				System.Diagnostics.Debug.Assert(bottomOrd < lookup.Length);
				bottomValue = values[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return values[slot];
			}
			
			public System.String[] GetValues()
			{
				return values;
			}
			
			public int GetBottomSlot()
			{
				return bottomSlot;
			}
			
			public System.String GetField()
			{
				return field;
			}
		}
		
		/// <summary>Sorts by field's natural String sort order.  All
		/// comparisons are done using String.compareTo, which is
		/// slow for medium to large result sets but possibly
		/// very fast for very small results sets. 
		/// </summary>
		public sealed class StringValComparator:FieldComparator
		{
			
			private System.String[] values;
			private System.String[] currentReaderValues;
			private System.String field;
			private System.String bottom;
			
			internal StringValComparator(int numHits, System.String field)
			{
				values = new System.String[numHits];
				this.field = field;
			}
			
			public override int Compare(int slot1, int slot2)
			{
				System.String val1 = values[slot1];
				System.String val2 = values[slot2];
				if (val1 == null)
				{
					if (val2 == null)
					{
						return 0;
					}
					return - 1;
				}
				else if (val2 == null)
				{
					return 1;
				}
				
				return String.CompareOrdinal(val1, val2);
			}
			
			public override int CompareBottom(int doc)
			{
				System.String val2 = currentReaderValues[doc];
				if (bottom == null)
				{
					if (val2 == null)
					{
						return 0;
					}
					return - 1;
				}
				else if (val2 == null)
				{
					return 1;
				}
				return String.CompareOrdinal(bottom, val2);
			}
			
			public override void  Copy(int slot, int doc)
			{
				values[slot] = currentReaderValues[doc];
			}
			
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				currentReaderValues = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetStrings(reader, field);
			}
			
			public override void  SetBottom(int bottom)
			{
				this.bottom = values[bottom];
			}
			
			public override System.IComparable Value(int slot)
			{
				return values[slot];
			}
		}
		
		protected internal static int BinarySearch(System.String[] a, System.String key)
		{
			return BinarySearch(a, key, 0, a.Length - 1);
		}
		
		protected internal static int BinarySearch(System.String[] a, System.String key, int low, int high)
		{
			
			while (low <= high)
			{
				int mid = SupportClass.Number.URShift((low + high), 1);
				System.String midVal = a[mid];
				int cmp;
				if (midVal != null)
				{
					cmp = String.CompareOrdinal(midVal, key);
				}
				else
				{
					cmp = - 1;
				}
				
				if (cmp < 0)
					low = mid + 1;
				else if (cmp > 0)
					high = mid - 1;
				else
					return mid;
			}
			return - (low + 1);
		}
		
		/// <summary> Compare hit at slot1 with hit at slot2.
		/// 
		/// </summary>
		/// <param name="slot1">first slot to compare
		/// </param>
		/// <param name="slot2">second slot to compare
		/// </param>
        /// <returns> any N &lt; 0 if slot2's value is sorted after
		/// slot1, any N > 0 if the slot2's value is sorted before
		/// slot1 and 0 if they are equal
		/// </returns>
		public abstract int Compare(int slot1, int slot2);
		
		/// <summary> Set the bottom slot, ie the "weakest" (sorted last)
		/// entry in the queue.  When {@link #compareBottom} is
		/// called, you should compare against this slot.  This
		/// will always be called before {@link #compareBottom}.
		/// 
		/// </summary>
		/// <param name="slot">the currently weakest (sorted last) slot in the queue
		/// </param>
		public abstract void  SetBottom(int slot);
		
		/// <summary> Compare the bottom of the queue with doc.  This will
		/// only invoked after setBottom has been called.  This
		/// should return the same result as {@link
		/// #Compare(int,int)}} as if bottom were slot1 and the new
		/// document were slot 2.
		/// 
		/// <p/>For a search that hits many results, this method
		/// will be the hotspot (invoked by far the most
		/// frequently).<p/>
		/// 
		/// </summary>
		/// <param name="doc">that was hit
		/// </param>
        /// <returns> any N &lt; 0 if the doc's value is sorted after
		/// the bottom entry (not competitive), any N > 0 if the
		/// doc's value is sorted before the bottom entry and 0 if
		/// they are equal.
		/// </returns>
		public abstract int CompareBottom(int doc);
		
		/// <summary> This method is called when a new hit is competitive.
		/// You should copy any state associated with this document
		/// that will be required for future comparisons, into the
		/// specified slot.
		/// 
		/// </summary>
		/// <param name="slot">which slot to copy the hit to
		/// </param>
		/// <param name="doc">docID relative to current reader
		/// </param>
		public abstract void  Copy(int slot, int doc);
		
		/// <summary> Set a new Reader. All doc correspond to the current Reader.
		/// 
		/// </summary>
		/// <param name="reader">current reader
		/// </param>
		/// <param name="docBase">docBase of this reader 
		/// </param>
		/// <throws>  IOException </throws>
		/// <throws>  IOException </throws>
		public abstract void  SetNextReader(IndexReader reader, int docBase);
		
		/// <summary>Sets the Scorer to use in case a document's score is
		/// needed.
		/// 
		/// </summary>
		/// <param name="scorer">Scorer instance that you should use to
		/// obtain the current hit's score, if necessary. 
		/// </param>
		public virtual void  SetScorer(Scorer scorer)
		{
			// Empty implementation since most comparators don't need the score. This
			// can be overridden by those that need it.
		}
		
		/// <summary> Return the actual value in the slot.
		/// 
		/// </summary>
		/// <param name="slot">the value
		/// </param>
		/// <returns> value in this slot upgraded to Comparable
		/// </returns>
		public abstract System.IComparable Value(int slot);
	}
}
