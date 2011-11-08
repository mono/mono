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
using OpenBitSet = Mono.Lucene.Net.Util.OpenBitSet;
using SpanQuery = Mono.Lucene.Net.Search.Spans.SpanQuery;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Constrains search results to only match those which also match a provided
	/// query. Also provides position information about where each document matches
	/// at the cost of extra space compared with the QueryWrapperFilter.
	/// There is an added cost to this above what is stored in a {@link QueryWrapperFilter}.  Namely,
	/// the position information for each matching document is stored.
	/// <p/>
	/// This filter does not cache.  See the {@link Mono.Lucene.Net.Search.CachingSpanFilter} for a wrapper that
	/// caches.
	/// 
	/// 
	/// </summary>
	/// <version>  $Id:$
	/// </version>
	[Serializable]
	public class SpanQueryFilter:SpanFilter
	{
		protected internal SpanQuery query;
		
		protected internal SpanQueryFilter()
		{
		}
		
		/// <summary>Constructs a filter which only matches documents matching
		/// <code>query</code>.
		/// </summary>
		/// <param name="query">The {@link Mono.Lucene.Net.Search.Spans.SpanQuery} to use as the basis for the Filter.
		/// </param>
		public SpanQueryFilter(SpanQuery query)
		{
			this.query = query;
		}
		
		public override DocIdSet GetDocIdSet(IndexReader reader)
		{
			SpanFilterResult result = BitSpans(reader);
			return result.GetDocIdSet();
		}
		
		public override SpanFilterResult BitSpans(IndexReader reader)
		{
			
			OpenBitSet bits = new OpenBitSet(reader.MaxDoc());
			Mono.Lucene.Net.Search.Spans.Spans spans = query.GetSpans(reader);
			System.Collections.IList tmp = new System.Collections.ArrayList(20);
			int currentDoc = - 1;
			SpanFilterResult.PositionInfo currentInfo = null;
			while (spans.Next())
			{
				int doc = spans.Doc();
				bits.Set(doc);
				if (currentDoc != doc)
				{
					currentInfo = new SpanFilterResult.PositionInfo(doc);
					tmp.Add(currentInfo);
					currentDoc = doc;
				}
				currentInfo.AddPosition(spans.Start(), spans.End());
			}
			return new SpanFilterResult(bits, tmp);
		}
		
		
		public virtual SpanQuery GetQuery()
		{
			return query;
		}
		
		public override System.String ToString()
		{
			return "SpanQueryFilter(" + query + ")";
		}
		
		public  override bool Equals(System.Object o)
		{
			return o is SpanQueryFilter && this.query.Equals(((SpanQueryFilter) o).query);
		}
		
		public override int GetHashCode()
		{
			return query.GetHashCode() ^ unchecked((int) 0x923F64B9);
		}
	}
}
