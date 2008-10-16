/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
using System.Runtime.InteropServices;
using Monodoc.Lucene.Net.Index;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>Constrains search results to only match those which also match a provided
	/// query.  Results are cached, so that searches after the first on the same
	/// index using this filter are much faster.
	/// 
	/// <p> This could be used, for example, with a {@link RangeQuery} on a suitably
	/// formatted date Field to implement date filtering.  One could re-use a single
	/// QueryFilter that matches, e.g., only documents modified within the last
	/// week.  The QueryFilter and RangeQuery would only need to be reconstructed
	/// once per day.
	/// 
	/// </summary>
	/// <version>  $Id: QueryFilter.java,v 1.6 2004/05/08 19:54:12 ehatcher Exp $
	/// </version>
	[Serializable]
	public class QueryFilter:Filter
	{
		private class AnonymousClassHitCollector:HitCollector
		{
			public AnonymousClassHitCollector(System.Collections.BitArray bits, QueryFilter enclosingInstance)
			{
				InitBlock(bits, enclosingInstance);
			}
			private void  InitBlock(System.Collections.BitArray bits, QueryFilter enclosingInstance)
			{
				this.bits = bits;
				this.enclosingInstance = enclosingInstance;
			}
			private System.Collections.BitArray bits;
			private QueryFilter enclosingInstance;
			public QueryFilter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override void  Collect(int doc, float score)
			{
				bits.Set(doc, true); // set bit for hit
			}
		}
		private Query query;
		[NonSerialized]
		private System.Collections.Hashtable cache = null;
		
		/// <summary>Constructs a filter which only matches documents matching
		/// <code>query</code>.
		/// </summary>
		public QueryFilter(Query query)
		{
			this.query = query;
		}
		
		public override System.Collections.BitArray Bits(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			
			if (cache == null)
			{
				cache = new System.Collections.Hashtable();
			}
			
			lock (cache.SyncRoot)
			{
				// check cache
				System.Collections.BitArray cached = (System.Collections.BitArray) cache[reader];
				if (cached != null)
				{
					return cached;
				}
			}
			
			System.Collections.BitArray bits = new System.Collections.BitArray((reader.MaxDoc() % 64 == 0?reader.MaxDoc() / 64:reader.MaxDoc() / 64 + 1) * 64);
			
			new IndexSearcher(reader).Search(query, new AnonymousClassHitCollector(bits, this));
			
			lock (cache.SyncRoot)
			{
				// update cache
				cache[reader] = bits;
			}
			
			return bits;
		}
		
		public override System.String ToString()
		{
			return "QueryFilter(" + query + ")";
		}
	}
}