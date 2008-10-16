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
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>An abstract base class for search implementations.
	/// Implements some common utility methods.
	/// </summary>
	public abstract class Searcher : Monodoc.Lucene.Net.Search.Searchable
	{
		public Searcher()
		{
			InitBlock();
		}
		private void  InitBlock()
		{
			similarity = Similarity.GetDefault();
		}
		/// <summary>Returns the documents matching <code>query</code>. </summary>
		public Hits Search(Query query)
		{
			return Search(query, (Filter) null);
		}
		
		/// <summary>Returns the documents matching <code>query</code> and
		/// <code>filter</code>. 
		/// </summary>
		public virtual Hits Search(Query query, Filter filter)
		{
			return new Hits(this, query, filter);
		}
		
		/// <summary>Returns documents matching <code>query</code> sorted by
		/// <code>sort</code>.
		/// </summary>
		public virtual Hits Search(Query query, Sort sort)
		{
			return new Hits(this, query, null, sort);
		}
		
		/// <summary>Returns documents matching <code>query</code> and <code>filter</code>,
		/// sorted by <code>sort</code>.
		/// </summary>
		public virtual Hits Search(Query query, Filter filter, Sort sort)
		{
			return new Hits(this, query, filter, sort);
		}
		
		/// <summary>Lower-level search API.
		/// 
		/// <p>{@link HitCollector#Collect(int,float)} is called for every non-zero
		/// scoring document.
		/// 
		/// <p>Applications should only use this if they need <i>all</i> of the
		/// matching documents.  The high-level search API ({@link
		/// Searcher#Search(Query)}) is usually more efficient, as it skips
		/// non-high-scoring hits.
		/// <p>Note: The <code>score</code> passed to this method is a raw score.
		/// In other words, the score will not necessarily be a float whose value is
		/// between 0 and 1.
		/// </summary>
		public virtual void  Search(Query query, HitCollector results)
		{
			Search(query, (Filter) null, results);
		}
		
		/// <summary>The Similarity implementation used by this searcher. </summary>
		private Similarity similarity;
		
		/// <summary>Expert: Set the Similarity implementation used by this Searcher.
		/// 
		/// </summary>
		/// <seealso cref="Similarity#SetDefault(Similarity)">
		/// </seealso>
		public virtual void  SetSimilarity(Similarity similarity)
		{
			this.similarity = similarity;
		}
		
		/// <summary>Expert: Return the Similarity implementation used by this Searcher.
		/// 
		/// <p>This defaults to the current value of {@link Similarity#GetDefault()}.
		/// </summary>
		public virtual Similarity GetSimilarity()
		{
			return this.similarity;
		}
		public abstract void  Close();
		public abstract Monodoc.Lucene.Net.Search.Explanation Explain(Monodoc.Lucene.Net.Search.Query param1, int param2);
		public abstract Monodoc.Lucene.Net.Search.TopFieldDocs Search(Monodoc.Lucene.Net.Search.Query param1, Monodoc.Lucene.Net.Search.Filter param2, int param3, Monodoc.Lucene.Net.Search.Sort param4);
		public abstract void  Search(Monodoc.Lucene.Net.Search.Query param1, Monodoc.Lucene.Net.Search.Filter param2, Monodoc.Lucene.Net.Search.HitCollector param3);
		public abstract int DocFreq(Monodoc.Lucene.Net.Index.Term param1);
		public abstract int MaxDoc();
		public abstract Monodoc.Lucene.Net.Search.Query Rewrite(Monodoc.Lucene.Net.Search.Query param1);
		public abstract Monodoc.Lucene.Net.Documents.Document Doc(int param1);
		public abstract Monodoc.Lucene.Net.Search.TopDocs Search(Monodoc.Lucene.Net.Search.Query param1, Monodoc.Lucene.Net.Search.Filter param2, int param3);
	}
}