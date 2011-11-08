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
using Term = Mono.Lucene.Net.Index.Term;
using TermDocs = Mono.Lucene.Net.Index.TermDocs;
using TermEnum = Mono.Lucene.Net.Index.TermEnum;
using OpenBitSet = Mono.Lucene.Net.Util.OpenBitSet;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A wrapper for {@link MultiTermQuery}, that exposes its
	/// functionality as a {@link Filter}.
	/// <p/>
	/// <code>MultiTermQueryWrapperFilter</code> is not designed to
	/// be used by itself. Normally you subclass it to provide a Filter
	/// counterpart for a {@link MultiTermQuery} subclass.
	/// <p/>
	/// For example, {@link TermRangeFilter} and {@link PrefixFilter} extend
	/// <code>MultiTermQueryWrapperFilter</code>.
	/// This class also provides the functionality behind
	/// {@link MultiTermQuery#CONSTANT_SCORE_FILTER_REWRITE};
	/// this is why it is not abstract.
	/// </summary>
	[Serializable]
	public class MultiTermQueryWrapperFilter:Filter
	{
		private class AnonymousClassTermGenerator:TermGenerator
		{
			public AnonymousClassTermGenerator(System.Collections.BitArray bitSet, MultiTermQueryWrapperFilter enclosingInstance)
			{
				InitBlock(bitSet, enclosingInstance);
			}
			private void  InitBlock(System.Collections.BitArray bitSet, MultiTermQueryWrapperFilter enclosingInstance)
			{
				this.bitSet = bitSet;
				this.enclosingInstance = enclosingInstance;
			}
			private System.Collections.BitArray bitSet;
			private MultiTermQueryWrapperFilter enclosingInstance;
			public MultiTermQueryWrapperFilter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override void  HandleDoc(int doc)
			{
				bitSet.Set(doc, true);
			}
		}

		private class AnonymousClassTermGenerator1:TermGenerator
		{
			public AnonymousClassTermGenerator1(Mono.Lucene.Net.Util.OpenBitSet bitSet, MultiTermQueryWrapperFilter enclosingInstance)
			{
				InitBlock(bitSet, enclosingInstance);
			}
			private void  InitBlock(Mono.Lucene.Net.Util.OpenBitSet bitSet, MultiTermQueryWrapperFilter enclosingInstance)
			{
				this.bitSet = bitSet;
				this.enclosingInstance = enclosingInstance;
			}
			private Mono.Lucene.Net.Util.OpenBitSet bitSet;
			private MultiTermQueryWrapperFilter enclosingInstance;
			public MultiTermQueryWrapperFilter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override void  HandleDoc(int doc)
			{
				bitSet.Set(doc);
			}
		}
		
		protected internal MultiTermQuery query;
		
		/// <summary> Wrap a {@link MultiTermQuery} as a Filter.</summary>
		protected internal MultiTermQueryWrapperFilter(MultiTermQuery query)
		{
			this.query = query;
		}
		
		//@Override
		public override System.String ToString()
		{
			// query.toString should be ok for the filter, too, if the query boost is 1.0f
			return query.ToString();
		}
		
		//@Override
		public  override bool Equals(System.Object o)
		{
			if (o == this)
				return true;
			if (o == null)
				return false;
			if (this.GetType().Equals(o.GetType()))
			{
				return this.query.Equals(((MultiTermQueryWrapperFilter) o).query);
			}
			return false;
		}
		
		//@Override
		public override int GetHashCode()
		{
			return query.GetHashCode();
		}
		
		/// <summary> Expert: Return the number of unique terms visited during execution of the filter.
		/// If there are many of them, you may consider using another filter type
		/// or optimize your total term count in index.
		/// <p/>This method is not thread safe, be sure to only call it when no filter is running!
		/// If you re-use the same filter instance for another
		/// search, be sure to first reset the term counter
		/// with {@link #clearTotalNumberOfTerms}.
		/// </summary>
		/// <seealso cref="clearTotalNumberOfTerms">
		/// </seealso>
		public virtual int GetTotalNumberOfTerms()
		{
			return query.GetTotalNumberOfTerms();
		}
		
		/// <summary> Expert: Resets the counting of unique terms.
		/// Do this before executing the filter.
		/// </summary>
		/// <seealso cref="getTotalNumberOfTerms">
		/// </seealso>
		public virtual void  ClearTotalNumberOfTerms()
		{
			query.ClearTotalNumberOfTerms();
		}
		
		internal abstract class TermGenerator
		{
            public virtual void Generate(MultiTermQuery query, IndexReader reader, TermEnum enumerator)
			{
				int[] docs = new int[32];
				int[] freqs = new int[32];
				TermDocs termDocs = reader.TermDocs();
				try
				{
					int termCount = 0;
					do 
					{
						Term term = enumerator.Term();
						if (term == null)
							break;
						termCount++;
						termDocs.Seek(term);
						while (true)
						{
							int count = termDocs.Read(docs, freqs);
							if (count != 0)
							{
								for (int i = 0; i < count; i++)
								{
									HandleDoc(docs[i]);
								}
							}
							else
							{
								break;
							}
						}
					}
					while (enumerator.Next());
					
					query.IncTotalNumberOfTerms(termCount); // {{Aroush-2.9}} is the use of 'temp' as is right?
				}
				finally
				{
					termDocs.Close();
				}
			}
			abstract public void  HandleDoc(int doc);
		}
		
		/// <summary> Returns a BitSet with true for documents which should be
		/// permitted in search results, and false for those that should
		/// not.
		/// </summary>
		/// <deprecated> Use {@link #GetDocIdSet(IndexReader)} instead.
		/// </deprecated>
		//@Override
        [Obsolete("Use GetDocIdSet(IndexReader) instead.")]
		public override System.Collections.BitArray Bits(IndexReader reader)
		{
			TermEnum enumerator = query.GetEnum(reader);
			try
			{
				System.Collections.BitArray bitSet = new System.Collections.BitArray((reader.MaxDoc() % 64 == 0?reader.MaxDoc() / 64:reader.MaxDoc() / 64 + 1) * 64);
				new AnonymousClassTermGenerator(bitSet, this).Generate(query, reader, enumerator);
				return bitSet;
			}
			finally
			{
				enumerator.Close();
			}
		}
		
		/// <summary> Returns a DocIdSet with documents that should be
		/// permitted in search results.
		/// </summary>
		//@Override
		public override DocIdSet GetDocIdSet(IndexReader reader)
		{
			TermEnum enumerator = query.GetEnum(reader);
			try
			{
				// if current term in enum is null, the enum is empty -> shortcut
				if (enumerator.Term() == null)
					return DocIdSet.EMPTY_DOCIDSET;
				// else fill into a OpenBitSet
				OpenBitSet bitSet = new OpenBitSet(reader.MaxDoc());
				new AnonymousClassTermGenerator1(bitSet, this).Generate(query, reader, enumerator);
				return bitSet;
			}
			finally
			{
				enumerator.Close();
			}
		}
	}
}
