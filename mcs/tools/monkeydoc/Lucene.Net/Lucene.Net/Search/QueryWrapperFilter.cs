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

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Constrains search results to only match those which also match a provided
	/// query.  
	/// 
	/// <p/> This could be used, for example, with a {@link TermRangeQuery} on a suitably
	/// formatted date field to implement date filtering.  One could re-use a single
	/// QueryFilter that matches, e.g., only documents modified within the last
	/// week.  The QueryFilter and TermRangeQuery would only need to be reconstructed
	/// once per day.
	/// 
	/// </summary>
	/// <version>  $Id:$
	/// </version>
	[Serializable]
	public class QueryWrapperFilter:Filter
	{
		private class AnonymousClassCollector:Collector
		{
			public AnonymousClassCollector(System.Collections.BitArray bits, QueryWrapperFilter enclosingInstance)
			{
				InitBlock(bits, enclosingInstance);
			}
			private void  InitBlock(System.Collections.BitArray bits, QueryWrapperFilter enclosingInstance)
			{
				this.bits = bits;
				this.enclosingInstance = enclosingInstance;
			}
			private System.Collections.BitArray bits;
			private QueryWrapperFilter enclosingInstance;
			public QueryWrapperFilter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private int base_Renamed = 0;
			public override void  SetScorer(Scorer scorer)
			{
				// score is not needed by this collector 
			}
			public override void  Collect(int doc)
			{
                for (int i = 0; doc + base_Renamed >= bits.Length; i =+ 64)
                {
                    bits.Length += i;
                }
                bits.Set(doc + base_Renamed, true); // set bit for hit
			}
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				base_Renamed = docBase;
			}
			public override bool AcceptsDocsOutOfOrder()
			{
				return true;
			}
		}
		private class AnonymousClassDocIdSet:DocIdSet
		{
			public AnonymousClassDocIdSet(Mono.Lucene.Net.Search.Weight weight, Mono.Lucene.Net.Index.IndexReader reader, QueryWrapperFilter enclosingInstance)
			{
				InitBlock(weight, reader, enclosingInstance);
			}
			private void  InitBlock(Mono.Lucene.Net.Search.Weight weight, Mono.Lucene.Net.Index.IndexReader reader, QueryWrapperFilter enclosingInstance)
			{
				this.weight = weight;
				this.reader = reader;
				this.enclosingInstance = enclosingInstance;
			}
			private Mono.Lucene.Net.Search.Weight weight;
			private Mono.Lucene.Net.Index.IndexReader reader;
			private QueryWrapperFilter enclosingInstance;
			public QueryWrapperFilter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override DocIdSetIterator Iterator()
			{
				return weight.Scorer(reader, true, false);
			}
			public override bool IsCacheable()
			{
				return false;
			}
		}
		private Query query;
		
		/// <summary>Constructs a filter which only matches documents matching
		/// <code>query</code>.
		/// </summary>
		public QueryWrapperFilter(Query query)
		{
			this.query = query;
		}
		
		/// <deprecated> Use {@link #GetDocIdSet(IndexReader)} instead.
		/// </deprecated>
        [Obsolete("Use GetDocIdSet(IndexReader) instead.")]
		public override System.Collections.BitArray Bits(IndexReader reader)
		{
			System.Collections.BitArray bits = new System.Collections.BitArray((reader.MaxDoc() % 64 == 0?reader.MaxDoc() / 64:reader.MaxDoc() / 64 + 1) * 64);
			
			new IndexSearcher(reader).Search(query, new AnonymousClassCollector(bits, this));
			return bits;
		}
		
		public override DocIdSet GetDocIdSet(IndexReader reader)
		{
			Weight weight = query.Weight(new IndexSearcher(reader));
			return new AnonymousClassDocIdSet(weight, reader, this);
		}
		
		public override System.String ToString()
		{
			return "QueryWrapperFilter(" + query + ")";
		}
		
		public  override bool Equals(System.Object o)
		{
			if (!(o is QueryWrapperFilter))
				return false;
			return this.query.Equals(((QueryWrapperFilter) o).query);
		}
		
		public override int GetHashCode()
		{
			return query.GetHashCode() ^ unchecked((int) 0x923F64B9);
		}
	}
}
