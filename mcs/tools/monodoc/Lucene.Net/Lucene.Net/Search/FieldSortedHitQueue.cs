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
using PriorityQueue = Mono.Lucene.Net.Util.PriorityQueue;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Expert: A hit queue for sorting by hits by terms in more than one field.
	/// Uses <code>FieldCache.DEFAULT</code> for maintaining internal term lookup tables.
	/// 
	/// <p/>Created: Dec 8, 2003 12:56:03 PM
	/// 
	/// </summary>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: FieldSortedHitQueue.java 803676 2009-08-12 19:31:38Z hossman $
	/// </version>
	/// <seealso cref="Searcher.Search(Query,Filter,int,Sort)">
	/// </seealso>
	/// <seealso cref="FieldCache">
	/// </seealso>
	/// <deprecated> see {@link FieldValueHitQueue}
	/// </deprecated>
    [Obsolete("see FieldValueHitQueue")]
	public class FieldSortedHitQueue:PriorityQueue
	{
		internal class AnonymousClassCache:FieldCacheImpl.Cache
		{
			
			protected internal override System.Object CreateValue(IndexReader reader, FieldCacheImpl.Entry entryKey)
			{
				FieldCacheImpl.Entry entry = (FieldCacheImpl.Entry) entryKey;
				System.String fieldname = entry.field;
				int type = entry.type;
				System.Globalization.CultureInfo locale = entry.locale;
				Mono.Lucene.Net.Search.Parser parser = null;
				SortComparatorSource factory = null;
				if (entry.custom is SortComparatorSource)
				{
					factory = (SortComparatorSource) entry.custom;
				}
				else
				{
					parser = (Mono.Lucene.Net.Search.Parser) entry.custom;
				}
				ScoreDocComparator comparator;
				switch (type)
				{
					
					case SortField.AUTO: 
						comparator = Mono.Lucene.Net.Search.FieldSortedHitQueue.ComparatorAuto(reader, fieldname);
						break;
					
					case SortField.INT: 
						comparator = Mono.Lucene.Net.Search.FieldSortedHitQueue.comparatorInt(reader, fieldname, (Mono.Lucene.Net.Search.IntParser) parser);
						break;
					
					case SortField.FLOAT: 
						comparator = Mono.Lucene.Net.Search.FieldSortedHitQueue.comparatorFloat(reader, fieldname, (Mono.Lucene.Net.Search.FloatParser) parser);
						break;
					
					case SortField.LONG: 
						comparator = Mono.Lucene.Net.Search.FieldSortedHitQueue.comparatorLong(reader, fieldname, (Mono.Lucene.Net.Search.LongParser) parser);
						break;
					
					case SortField.DOUBLE: 
						comparator = Mono.Lucene.Net.Search.FieldSortedHitQueue.comparatorDouble(reader, fieldname, (Mono.Lucene.Net.Search.DoubleParser) parser);
						break;
					
					case SortField.SHORT: 
						comparator = Mono.Lucene.Net.Search.FieldSortedHitQueue.comparatorShort(reader, fieldname, (Mono.Lucene.Net.Search.ShortParser) parser);
						break;
					
					case SortField.BYTE: 
						comparator = Mono.Lucene.Net.Search.FieldSortedHitQueue.comparatorByte(reader, fieldname, (Mono.Lucene.Net.Search.ByteParser) parser);
						break;
					
					case SortField.STRING: 
						if (locale != null)
							comparator = Mono.Lucene.Net.Search.FieldSortedHitQueue.comparatorStringLocale(reader, fieldname, locale);
						else
							comparator = Mono.Lucene.Net.Search.FieldSortedHitQueue.comparatorString(reader, fieldname);
						break;
					
					case SortField.CUSTOM: 
						comparator = factory.NewComparator(reader, fieldname);
						break;
					
					default: 
						throw new System.SystemException("unknown field type: " + type);
					
				}
				return comparator;
			}
		}
		private class AnonymousClassScoreDocComparator : ScoreDocComparator
		{
			public AnonymousClassScoreDocComparator(sbyte[] fieldOrder)
			{
				InitBlock(fieldOrder);
			}
			private void  InitBlock(sbyte[] fieldOrder)
			{
				this.fieldOrder = fieldOrder;
			}
			private sbyte[] fieldOrder;
			
			public int Compare(ScoreDoc i, ScoreDoc j)
			{
				int fi = fieldOrder[i.doc];
				int fj = fieldOrder[j.doc];
				if (fi < fj)
					return - 1;
				if (fi > fj)
					return 1;
				return 0;
			}
			
			public virtual System.IComparable SortValue(ScoreDoc i)
			{
				return (sbyte) fieldOrder[i.doc];
			}
			
			public virtual int SortType()
			{
				return SortField.BYTE;
			}
		}
		private class AnonymousClassScoreDocComparator1 : ScoreDocComparator
		{
			public AnonymousClassScoreDocComparator1(short[] fieldOrder)
			{
				InitBlock(fieldOrder);
			}
			private void  InitBlock(short[] fieldOrder)
			{
				this.fieldOrder = fieldOrder;
			}
			private short[] fieldOrder;
			
			public int Compare(ScoreDoc i, ScoreDoc j)
			{
				int fi = fieldOrder[i.doc];
				int fj = fieldOrder[j.doc];
				if (fi < fj)
					return - 1;
				if (fi > fj)
					return 1;
				return 0;
			}
			
			public virtual System.IComparable SortValue(ScoreDoc i)
			{
				return (short) fieldOrder[i.doc];
			}
			
			public virtual int SortType()
			{
				return SortField.SHORT;
			}
		}
		private class AnonymousClassScoreDocComparator2 : ScoreDocComparator
		{
			public AnonymousClassScoreDocComparator2(int[] fieldOrder)
			{
				InitBlock(fieldOrder);
			}
			private void  InitBlock(int[] fieldOrder)
			{
				this.fieldOrder = fieldOrder;
			}
			private int[] fieldOrder;
			
			public int Compare(ScoreDoc i, ScoreDoc j)
			{
				int fi = fieldOrder[i.doc];
				int fj = fieldOrder[j.doc];
				if (fi < fj)
					return - 1;
				if (fi > fj)
					return 1;
				return 0;
			}
			
			public virtual System.IComparable SortValue(ScoreDoc i)
			{
				return (System.Int32) fieldOrder[i.doc];
			}
			
			public virtual int SortType()
			{
				return SortField.INT;
			}
		}
		private class AnonymousClassScoreDocComparator3 : ScoreDocComparator
		{
			public AnonymousClassScoreDocComparator3(long[] fieldOrder)
			{
				InitBlock(fieldOrder);
			}
			private void  InitBlock(long[] fieldOrder)
			{
				this.fieldOrder = fieldOrder;
			}
			private long[] fieldOrder;
			
			public int Compare(ScoreDoc i, ScoreDoc j)
			{
				long li = fieldOrder[i.doc];
				long lj = fieldOrder[j.doc];
				if (li < lj)
					return - 1;
				if (li > lj)
					return 1;
				return 0;
			}
			
			public virtual System.IComparable SortValue(ScoreDoc i)
			{
				return (long) fieldOrder[i.doc];
			}
			
			public virtual int SortType()
			{
				return SortField.LONG;
			}
		}
		private class AnonymousClassScoreDocComparator4 : ScoreDocComparator
		{
			public AnonymousClassScoreDocComparator4(float[] fieldOrder)
			{
				InitBlock(fieldOrder);
			}
			private void  InitBlock(float[] fieldOrder)
			{
				this.fieldOrder = fieldOrder;
			}
			private float[] fieldOrder;
			
			public int Compare(ScoreDoc i, ScoreDoc j)
			{
				float fi = fieldOrder[i.doc];
				float fj = fieldOrder[j.doc];
				if (fi < fj)
					return - 1;
				if (fi > fj)
					return 1;
				return 0;
			}
			
			public virtual System.IComparable SortValue(ScoreDoc i)
			{
				return (float) fieldOrder[i.doc];
			}
			
			public virtual int SortType()
			{
				return SortField.FLOAT;
			}
		}
		private class AnonymousClassScoreDocComparator5 : ScoreDocComparator
		{
			public AnonymousClassScoreDocComparator5(double[] fieldOrder)
			{
				InitBlock(fieldOrder);
			}
			private void  InitBlock(double[] fieldOrder)
			{
				this.fieldOrder = fieldOrder;
			}
			private double[] fieldOrder;
			
			public int Compare(ScoreDoc i, ScoreDoc j)
			{
				double di = fieldOrder[i.doc];
				double dj = fieldOrder[j.doc];
				if (di < dj)
					return - 1;
				if (di > dj)
					return 1;
				return 0;
			}
			
			public virtual System.IComparable SortValue(ScoreDoc i)
			{
				return (double) fieldOrder[i.doc];
			}
			
			public virtual int SortType()
			{
				return SortField.DOUBLE;
			}
		}
		private class AnonymousClassScoreDocComparator6 : ScoreDocComparator
		{
			public AnonymousClassScoreDocComparator6(Mono.Lucene.Net.Search.StringIndex index)
			{
				InitBlock(index);
			}
			private void  InitBlock(Mono.Lucene.Net.Search.StringIndex index)
			{
				this.index = index;
			}
			private Mono.Lucene.Net.Search.StringIndex index;
			
			public int Compare(ScoreDoc i, ScoreDoc j)
			{
				int fi = index.order[i.doc];
				int fj = index.order[j.doc];
				if (fi < fj)
					return - 1;
				if (fi > fj)
					return 1;
				return 0;
			}
			
			public virtual System.IComparable SortValue(ScoreDoc i)
			{
				return index.lookup[index.order[i.doc]];
			}
			
			public virtual int SortType()
			{
				return SortField.STRING;
			}
		}
		private class AnonymousClassScoreDocComparator7 : ScoreDocComparator
		{
			public AnonymousClassScoreDocComparator7(System.String[] index, System.Globalization.CompareInfo collator)
			{
				InitBlock(index, collator);
			}
			private void  InitBlock(System.String[] index, System.Globalization.CompareInfo collator)
			{
				this.index = index;
				this.collator = collator;
			}
			private System.String[] index;
			private System.Globalization.CompareInfo collator;
			
			public int Compare(ScoreDoc i, ScoreDoc j)
			{
				System.String is_Renamed = index[i.doc];
				System.String js = index[j.doc];
				if ((System.Object) is_Renamed == (System.Object) js)
				{
					return 0;
				}
				else if (is_Renamed == null)
				{
					return - 1;
				}
				else if (js == null)
				{
					return 1;
				}
				else
				{
					return collator.Compare(is_Renamed.ToString(), js.ToString());
				}
			}
			
			public virtual System.IComparable SortValue(ScoreDoc i)
			{
				return index[i.doc];
			}
			
			public virtual int SortType()
			{
				return SortField.STRING;
			}
		}
		
		/// <summary> Creates a hit queue sorted by the given list of fields.</summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fields">Fieldable names, in priority order (highest priority first).  Cannot be <code>null</code> or empty.
		/// </param>
		/// <param name="size"> The number of hits to retain.  Must be greater than zero.
		/// </param>
		/// <throws>  IOException </throws>
		public FieldSortedHitQueue(IndexReader reader, SortField[] fields, int size)
		{
			int n = fields.Length;
			comparators = new ScoreDocComparator[n];
			this.fields = new SortField[n];
			for (int i = 0; i < n; ++i)
			{
				System.String fieldname = fields[i].GetField();
				comparators[i] = GetCachedComparator(reader, fieldname, fields[i].GetType(), fields[i].GetParser(), fields[i].GetLocale(), fields[i].GetFactory());
				// new SortField instances must only be created when auto-detection is in use
				if (fields[i].GetType() == SortField.AUTO)
				{
					if (comparators[i].SortType() == SortField.STRING)
					{
						this.fields[i] = new SortField(fieldname, fields[i].GetLocale(), fields[i].GetReverse());
					}
					else
					{
						this.fields[i] = new SortField(fieldname, comparators[i].SortType(), fields[i].GetReverse());
					}
				}
				else
				{
					System.Diagnostics.Debug.Assert(comparators [i].SortType() == fields [i].GetType());
					this.fields[i] = fields[i];
				}
			}
			Initialize(size);
		}
		
		
		/// <summary>Stores a comparator corresponding to each field being sorted by </summary>
		protected internal ScoreDocComparator[] comparators;
		
		/// <summary>Stores the sort criteria being used. </summary>
		protected internal SortField[] fields;
		
		/// <summary>Stores the maximum score value encountered, needed for normalizing. </summary>
		protected internal float maxscore = System.Single.NegativeInfinity;
		
		/// <summary>returns the maximum score encountered by elements inserted via insert()</summary>
		public virtual float GetMaxScore()
		{
			return maxscore;
		}
		
		// Update maxscore.
		private void  UpdateMaxScore(FieldDoc fdoc)
		{
			maxscore = System.Math.Max(maxscore, fdoc.score);
		}
		
		// The signature of this method takes a FieldDoc in order to avoid
		// the unneeded cast to retrieve the score.
		// inherit javadoc
		public virtual bool Insert(FieldDoc fdoc)
		{
			UpdateMaxScore(fdoc);
			return base.Insert(fdoc);
		}
		
		// This overrides PriorityQueue.insert() so that insert(FieldDoc) that
		// keeps track of the score isn't accidentally bypassed.  
		// inherit javadoc
        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Util.PriorityQueue.Insert(object)")]
		public override bool Insert(System.Object fdoc)
		{
			return Insert((FieldDoc) fdoc);
		}
		
		// This overrides PriorityQueue.insertWithOverflow() so that
		// updateMaxScore(FieldDoc) that keeps track of the score isn't accidentally
		// bypassed.
		public override System.Object InsertWithOverflow(System.Object element)
		{
			UpdateMaxScore((FieldDoc) element);
			return base.InsertWithOverflow(element);
		}
		
		/// <summary> Returns whether <code>a</code> is less relevant than <code>b</code>.</summary>
		/// <param name="a">ScoreDoc
		/// </param>
		/// <param name="b">ScoreDoc
		/// </param>
		/// <returns> <code>true</code> if document <code>a</code> should be sorted after document <code>b</code>.
		/// </returns>
		public override bool LessThan(System.Object a, System.Object b)
		{
			ScoreDoc docA = (ScoreDoc) a;
			ScoreDoc docB = (ScoreDoc) b;
			
			// run comparators
			int n = comparators.Length;
			int c = 0;
			for (int i = 0; i < n && c == 0; ++i)
			{
				c = (fields[i].reverse)?comparators[i].Compare(docB, docA):comparators[i].Compare(docA, docB);
			}
			// avoid random sort order that could lead to duplicates (bug #31241):
			if (c == 0)
				return docA.doc > docB.doc;
			return c > 0;
		}
		
		
		/// <summary> Given a FieldDoc object, stores the values used
		/// to sort the given document.  These values are not the raw
		/// values out of the index, but the internal representation
		/// of them.  This is so the given search hit can be collated
		/// by a MultiSearcher with other search hits.
		/// </summary>
		/// <param name="doc"> The FieldDoc to store sort values into.
		/// </param>
		/// <returns>  The same FieldDoc passed in.
		/// </returns>
		/// <seealso cref="Searchable.Search(Weight,Filter,int,Sort)">
		/// </seealso>
		internal virtual FieldDoc FillFields(FieldDoc doc)
		{
			int n = comparators.Length;
			System.IComparable[] fields = new System.IComparable[n];
			for (int i = 0; i < n; ++i)
				fields[i] = comparators[i].SortValue(doc);
			doc.fields = fields;
			//if (maxscore > 1.0f) doc.score /= maxscore;   // normalize scores
			return doc;
		}
		
		
		/// <summary>Returns the SortFields being used by this hit queue. </summary>
		internal virtual SortField[] GetFields()
		{
			return fields;
		}
		
		internal static ScoreDocComparator GetCachedComparator(IndexReader reader, System.String field, int type, Mono.Lucene.Net.Search.Parser parser, System.Globalization.CultureInfo locale, SortComparatorSource factory)
		{
			if (type == SortField.DOC)
				return Mono.Lucene.Net.Search.ScoreDocComparator_Fields.INDEXORDER;
			if (type == SortField.SCORE)
				return Mono.Lucene.Net.Search.ScoreDocComparator_Fields.RELEVANCE;
			FieldCacheImpl.Entry entry = (factory != null)?new FieldCacheImpl.Entry(field, factory):((parser != null)?new FieldCacheImpl.Entry(field, type, parser):new FieldCacheImpl.Entry(field, type, locale));
			return (ScoreDocComparator) Comparators.Get(reader, entry);
		}
		
		/// <summary>Internal cache of comparators. Similar to FieldCache, only
		/// caches comparators instead of term values. 
		/// </summary>
		internal static readonly FieldCacheImpl.Cache Comparators;
		
		/// <summary> Returns a comparator for sorting hits according to a field containing bytes.</summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fieldname"> Fieldable containing integer values.
		/// </param>
		/// <returns>  Comparator for sorting hits.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		internal static ScoreDocComparator comparatorByte(IndexReader reader, System.String fieldname, Mono.Lucene.Net.Search.ByteParser parser)
		{
			System.String field = String.Intern(fieldname);
			sbyte[] fieldOrder = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetBytes(reader, field, parser);
			return new AnonymousClassScoreDocComparator(fieldOrder);
		}
		
		/// <summary> Returns a comparator for sorting hits according to a field containing shorts.</summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fieldname"> Fieldable containing integer values.
		/// </param>
		/// <returns>  Comparator for sorting hits.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		internal static ScoreDocComparator comparatorShort(IndexReader reader, System.String fieldname, Mono.Lucene.Net.Search.ShortParser parser)
		{
			System.String field = String.Intern(fieldname);
			short[] fieldOrder = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetShorts(reader, field, parser);
			return new AnonymousClassScoreDocComparator1(fieldOrder);
		}
		
		/// <summary> Returns a comparator for sorting hits according to a field containing integers.</summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fieldname"> Fieldable containing integer values.
		/// </param>
		/// <returns>  Comparator for sorting hits.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		internal static ScoreDocComparator comparatorInt(IndexReader reader, System.String fieldname, Mono.Lucene.Net.Search.IntParser parser)
		{
			System.String field = String.Intern(fieldname);
			int[] fieldOrder = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetInts(reader, field, parser);
			return new AnonymousClassScoreDocComparator2(fieldOrder);
		}
		
		/// <summary> Returns a comparator for sorting hits according to a field containing integers.</summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fieldname"> Fieldable containing integer values.
		/// </param>
		/// <returns>  Comparator for sorting hits.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		internal static ScoreDocComparator comparatorLong(IndexReader reader, System.String fieldname, Mono.Lucene.Net.Search.LongParser parser)
		{
			System.String field = String.Intern(fieldname);
			long[] fieldOrder = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetLongs(reader, field, parser);
			return new AnonymousClassScoreDocComparator3(fieldOrder);
		}
		
		
		/// <summary> Returns a comparator for sorting hits according to a field containing floats.</summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fieldname"> Fieldable containing float values.
		/// </param>
		/// <returns>  Comparator for sorting hits.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		internal static ScoreDocComparator comparatorFloat(IndexReader reader, System.String fieldname, Mono.Lucene.Net.Search.FloatParser parser)
		{
			System.String field = String.Intern(fieldname);
			float[] fieldOrder = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetFloats(reader, field, parser);
			return new AnonymousClassScoreDocComparator4(fieldOrder);
		}
		
		/// <summary> Returns a comparator for sorting hits according to a field containing doubles.</summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fieldname"> Fieldable containing float values.
		/// </param>
		/// <returns>  Comparator for sorting hits.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		internal static ScoreDocComparator comparatorDouble(IndexReader reader, System.String fieldname, Mono.Lucene.Net.Search.DoubleParser parser)
		{
			System.String field = String.Intern(fieldname);
			double[] fieldOrder = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetDoubles(reader, field, parser);
			return new AnonymousClassScoreDocComparator5(fieldOrder);
		}
		
		/// <summary> Returns a comparator for sorting hits according to a field containing strings.</summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fieldname"> Fieldable containing string values.
		/// </param>
		/// <returns>  Comparator for sorting hits.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		internal static ScoreDocComparator comparatorString(IndexReader reader, System.String fieldname)
		{
			System.String field = String.Intern(fieldname);
			Mono.Lucene.Net.Search.StringIndex index = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetStringIndex(reader, field);
			return new AnonymousClassScoreDocComparator6(index);
		}
		
		/// <summary> Returns a comparator for sorting hits according to a field containing strings.</summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fieldname"> Fieldable containing string values.
		/// </param>
		/// <returns>  Comparator for sorting hits.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		internal static ScoreDocComparator comparatorStringLocale(IndexReader reader, System.String fieldname, System.Globalization.CultureInfo locale)
		{
			System.Globalization.CompareInfo collator = locale.CompareInfo;
			System.String field = String.Intern(fieldname);
			System.String[] index = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetStrings(reader, field);
			return new AnonymousClassScoreDocComparator7(index, collator);
		}
		
		/// <summary> Returns a comparator for sorting hits according to values in the given field.
		/// The terms in the field are looked at to determine whether they contain integers,
		/// floats or strings.  Once the type is determined, one of the other static methods
		/// in this class is called to get the comparator.
		/// </summary>
		/// <param name="reader"> Index to use.
		/// </param>
		/// <param name="fieldname"> Fieldable containing values.
		/// </param>
		/// <returns>  Comparator for sorting hits.
		/// </returns>
		/// <throws>  IOException If an error occurs reading the index. </throws>
		internal static ScoreDocComparator ComparatorAuto(IndexReader reader, System.String fieldname)
		{
			System.String field = String.Intern(fieldname);
			System.Object lookupArray = Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT.GetAuto(reader, field);
			if (lookupArray is Mono.Lucene.Net.Search.StringIndex)
			{
				return comparatorString(reader, field);
			}
			else if (lookupArray is int[])
			{
				return comparatorInt(reader, field, null);
			}
			else if (lookupArray is long[])
			{
				return comparatorLong(reader, field, null);
			}
			else if (lookupArray is float[])
			{
				return comparatorFloat(reader, field, null);
			}
			else if (lookupArray is System.String[])
			{
				return comparatorString(reader, field);
			}
			else
			{
				throw new System.SystemException("unknown data type in field '" + field + "'");
			}
		}
		static FieldSortedHitQueue()
		{
			Comparators = new AnonymousClassCache();
		}
	}
}
