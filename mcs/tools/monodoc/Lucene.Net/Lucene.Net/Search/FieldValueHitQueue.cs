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

using PriorityQueue = Mono.Lucene.Net.Util.PriorityQueue;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Expert: A hit queue for sorting by hits by terms in more than one field.
	/// Uses <code>FieldCache.DEFAULT</code> for maintaining
	/// internal term lookup tables.
	/// 
	/// This class will not resolve SortField.AUTO types, and expects the type
	/// of all SortFields used for construction to already have been resolved. 
	/// {@link SortField#DetectFieldType(IndexReader, String)} is a utility method which
	/// may be used for field type detection.
	/// 
	/// <b>NOTE:</b> This API is experimental and might change in
	/// incompatible ways in the next release.
	/// 
	/// </summary>
	/// <since> 2.9
	/// </since>
	/// <version>  $Id:
	/// </version>
	/// <seealso cref="Searcher.Search(Query,Filter,int,Sort)">
	/// </seealso>
	/// <seealso cref="FieldCache">
	/// </seealso>
	public abstract class FieldValueHitQueue:PriorityQueue
	{
		
		internal sealed class Entry
		{
			internal int slot;
			internal int docID;
			internal float score;
			
			internal Entry(int slot, int docID, float score)
			{
				this.slot = slot;
				this.docID = docID;
				this.score = score;
			}
			
			public override System.String ToString()
			{
				return "slot:" + slot + " docID:" + docID + " score=" + score;
			}
		}
		
		/// <summary> An implementation of {@link FieldValueHitQueue} which is optimized in case
		/// there is just one comparator.
		/// </summary>
		private sealed class OneComparatorFieldValueHitQueue:FieldValueHitQueue
		{
			
			private FieldComparator comparator;
			private int oneReverseMul;
			
			public OneComparatorFieldValueHitQueue(SortField[] fields, int size):base(fields)
			{
				if (fields.Length == 0)
				{
					throw new System.ArgumentException("Sort must contain at least one field");
				}
				
				SortField field = fields[0];
				// AUTO is resolved before we are called
				System.Diagnostics.Debug.Assert(field.GetType() != SortField.AUTO);
				comparator = field.GetComparator(size, 0);
				oneReverseMul = field.reverse?- 1:1;
				
				comparators[0] = comparator;
				reverseMul[0] = oneReverseMul;
				
				Initialize(size);
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
				Entry hitA = (Entry) a;
				Entry hitB = (Entry) b;
				
				System.Diagnostics.Debug.Assert(hitA != hitB);
				System.Diagnostics.Debug.Assert(hitA.slot != hitB.slot);
				
				int c = oneReverseMul * comparator.Compare(hitA.slot, hitB.slot);
				if (c != 0)
				{
					return c > 0;
				}
				
				// avoid random sort order that could lead to duplicates (bug #31241):
				return hitA.docID > hitB.docID;
			}
		}
		
		/// <summary> An implementation of {@link FieldValueHitQueue} which is optimized in case
		/// there is more than one comparator.
		/// </summary>
		private sealed class MultiComparatorsFieldValueHitQueue:FieldValueHitQueue
		{
			
			public MultiComparatorsFieldValueHitQueue(SortField[] fields, int size):base(fields)
			{
				
				int numComparators = comparators.Length;
				for (int i = 0; i < numComparators; ++i)
				{
					SortField field = fields[i];
					
					// AUTO is resolved before we are called
					System.Diagnostics.Debug.Assert(field.GetType() != SortField.AUTO);
					
					reverseMul[i] = field.reverse?- 1:1;
					comparators[i] = field.GetComparator(size, i);
				}
				
				Initialize(size);
			}
			
			public override bool LessThan(System.Object a, System.Object b)
			{
				Entry hitA = (Entry) a;
				Entry hitB = (Entry) b;
				
				System.Diagnostics.Debug.Assert(hitA != hitB);
				System.Diagnostics.Debug.Assert(hitA.slot != hitB.slot);
				
				int numComparators = comparators.Length;
				for (int i = 0; i < numComparators; ++i)
				{
					int c = reverseMul[i] * comparators[i].Compare(hitA.slot, hitB.slot);
					if (c != 0)
					{
						// Short circuit
						return c > 0;
					}
				}
				
				// avoid random sort order that could lead to duplicates (bug #31241):
				return hitA.docID > hitB.docID;
			}
		}
		
		// prevent instantiation and extension.
		private FieldValueHitQueue(SortField[] fields)
		{
			// When we get here, fields.length is guaranteed to be > 0, therefore no
			// need to check it again.
			
			// All these are required by this class's API - need to return arrays.
			// Therefore even in the case of a single comparator, create an array
			// anyway.
			this.fields = fields;
			int numComparators = fields.Length;
			comparators = new FieldComparator[numComparators];
			reverseMul = new int[numComparators];
		}
		
		/// <summary> Creates a hit queue sorted by the given list of fields.
		/// 
		/// <p/><b>NOTE</b>: The instances returned by this method
		/// pre-allocate a full array of length <code>numHits</code>.
		/// 
		/// </summary>
		/// <param name="fields">SortField array we are sorting by in priority order (highest
		/// priority first); cannot be <code>null</code> or empty
		/// </param>
		/// <param name="size">The number of hits to retain. Must be greater than zero.
		/// </param>
		/// <throws>  IOException </throws>
		public static FieldValueHitQueue Create(SortField[] fields, int size)
		{
			
			if (fields.Length == 0)
			{
				throw new System.ArgumentException("Sort must contain at least one field");
			}
			
			if (fields.Length == 1)
			{
				return new OneComparatorFieldValueHitQueue(fields, size);
			}
			else
			{
				return new MultiComparatorsFieldValueHitQueue(fields, size);
			}
		}
		
		internal virtual FieldComparator[] GetComparators()
		{
			return comparators;
		}
		
		internal virtual int[] GetReverseMul()
		{
			return reverseMul;
		}
		
		/// <summary>Stores the sort criteria being used. </summary>
		protected internal SortField[] fields;
		protected internal FieldComparator[] comparators;
		protected internal int[] reverseMul;
		
		public abstract override bool LessThan(System.Object a, System.Object b);
		
		/// <summary> Given a queue Entry, creates a corresponding FieldDoc
		/// that contains the values used to sort the given document.
		/// These values are not the raw values out of the index, but the internal
		/// representation of them. This is so the given search hit can be collated by
		/// a MultiSearcher with other search hits.
		/// 
		/// </summary>
		/// <param name="entry">The Entry used to create a FieldDoc
		/// </param>
		/// <returns> The newly created FieldDoc
		/// </returns>
		/// <seealso cref="Searchable.Search(Weight,Filter,int,Sort)">
		/// </seealso>
		internal virtual FieldDoc FillFields(Entry entry)
		{
			int n = comparators.Length;
			System.IComparable[] fields = new System.IComparable[n];
			for (int i = 0; i < n; ++i)
			{
				fields[i] = comparators[i].Value(entry.slot);
			}
			//if (maxscore > 1.0f) doc.score /= maxscore;   // normalize scores
			return new FieldDoc(entry.docID, entry.score, fields);
		}
		
		/// <summary>Returns the SortFields being used by this hit queue. </summary>
		internal virtual SortField[] GetFields()
		{
			return fields;
		}
	}
}
