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

using Document = Mono.Lucene.Net.Documents.Document;
using CorruptIndexException = Mono.Lucene.Net.Index.CorruptIndexException;
using Term = Mono.Lucene.Net.Index.Term;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> An abstract base class for search implementations. Implements the main search
	/// methods.
	/// 
	/// <p/>
	/// Note that you can only access hits from a Searcher as long as it is not yet
	/// closed, otherwise an IOException will be thrown.
	/// </summary>
	public abstract class Searcher : System.MarshalByRefObject, Searchable, System.IDisposable
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
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		/// <deprecated> Hits will be removed in Lucene 3.0. Use
		/// {@link #Search(Query, Filter, int)} instead.
		/// </deprecated>
        [Obsolete("Hits will be removed in Lucene 3.0. Use Search(Query, Filter, int) instead")]
		public Hits Search(Query query)
		{
			return Search(query, (Filter) null);
		}
		
		/// <summary>Returns the documents matching <code>query</code> and
		/// <code>filter</code>.
		/// </summary>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		/// <deprecated> Hits will be removed in Lucene 3.0. Use
		/// {@link #Search(Query, Filter, int)} instead.
		/// </deprecated>
        [Obsolete("Hits will be removed in Lucene 3.0. Use Search(Query, Filter, int) instead")]
		public virtual Hits Search(Query query, Filter filter)
		{
			return new Hits(this, query, filter);
		}
		
		/// <summary>Returns documents matching <code>query</code> sorted by
		/// <code>sort</code>.
		/// </summary>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		/// <deprecated> Hits will be removed in Lucene 3.0. Use 
		/// {@link #Search(Query, Filter, int, Sort)} instead.
		/// </deprecated>
        [Obsolete("Hits will be removed in Lucene 3.0. Use Search(Query, Filter, int, Sort) instead")]
		public virtual Hits Search(Query query, Sort sort)
		{
			return new Hits(this, query, null, sort);
		}
		
		/// <summary>Returns documents matching <code>query</code> and <code>filter</code>,
		/// sorted by <code>sort</code>.
		/// </summary>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		/// <deprecated> Hits will be removed in Lucene 3.0. Use 
		/// {@link #Search(Query, Filter, int, Sort)} instead.
		/// </deprecated>
        [Obsolete("Hits will be removed in Lucene 3.0. Use Search(Query, Filter, int, Sort) instead")]
		public virtual Hits Search(Query query, Filter filter, Sort sort)
		{
			return new Hits(this, query, filter, sort);
		}
		
		/// <summary>Search implementation with arbitrary sorting.  Finds
		/// the top <code>n</code> hits for <code>query</code>, applying
		/// <code>filter</code> if non-null, and sorting the hits by the criteria in
		/// <code>sort</code>.
		/// 
		/// <p/>NOTE: this does not compute scores by default; use
		/// {@link IndexSearcher#setDefaultFieldSortScoring} to enable scoring.
		/// 
		/// </summary>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		public virtual TopFieldDocs Search(Query query, Filter filter, int n, Sort sort)
		{
			return Search(CreateWeight(query), filter, n, sort);
		}
		
		/// <summary>Lower-level search API.
		/// 
		/// <p/>{@link HitCollector#Collect(int,float)} is called for every matching
		/// document.
		/// 
		/// <p/>Applications should only use this if they need <i>all</i> of the
		/// matching documents.  The high-level search API ({@link
		/// Searcher#Search(Query)}) is usually more efficient, as it skips
		/// non-high-scoring hits.
		/// <p/>Note: The <code>score</code> passed to this method is a raw score.
		/// In other words, the score will not necessarily be a float whose value is
		/// between 0 and 1.
		/// </summary>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		/// <deprecated> use {@link #Search(Query, Collector)} instead.
		/// </deprecated>
        [Obsolete("use Search(Query, Collector) instead.")]
		public virtual void  Search(Query query, HitCollector results)
		{
			Search(CreateWeight(query), null, new HitCollectorWrapper(results));
		}
		
		/// <summary>Lower-level search API.
		/// 
		/// <p/>{@link Collector#Collect(int)} is called for every matching document.
		/// 
		/// <p/>Applications should only use this if they need <i>all</i> of the matching
		/// documents. The high-level search API ({@link Searcher#Search(Query, int)}
		/// ) is usually more efficient, as it skips non-high-scoring hits.
		/// <p/>Note: The <code>score</code> passed to this method is a raw score.
		/// In other words, the score will not necessarily be a float whose value is
		/// between 0 and 1.
		/// </summary>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		public virtual void  Search(Query query, Collector results)
		{
			Search(CreateWeight(query), null, results);
		}
		
		/// <summary>Lower-level search API.
		/// 
		/// <p/>{@link HitCollector#Collect(int,float)} is called for every matching
		/// document.
		/// <br/>HitCollector-based access to remote indexes is discouraged.
		/// 
		/// <p/>Applications should only use this if they need <i>all</i> of the
		/// matching documents.  The high-level search API ({@link
		/// Searcher#Search(Query, Filter, int)}) is usually more efficient, as it skips
		/// non-high-scoring hits.
		/// 
		/// </summary>
		/// <param name="query">to match documents
		/// </param>
		/// <param name="filter">if non-null, used to permit documents to be collected.
		/// </param>
		/// <param name="results">to receive hits
		/// </param>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		/// <deprecated> use {@link #Search(Query, Filter, Collector)} instead.
		/// </deprecated>
        [Obsolete("use Search(Query, Filter, Collector) instead.")]
		public virtual void  Search(Query query, Filter filter, HitCollector results)
		{
			Search(CreateWeight(query), filter, new HitCollectorWrapper(results));
		}
		
		/// <summary>Lower-level search API.
		/// 
		/// <p/>{@link Collector#Collect(int)} is called for every matching
		/// document.
		/// <br/>Collector-based access to remote indexes is discouraged.
		/// 
		/// <p/>Applications should only use this if they need <i>all</i> of the
		/// matching documents.  The high-level search API ({@link
		/// Searcher#Search(Query, Filter, int)}) is usually more efficient, as it skips
		/// non-high-scoring hits.
		/// 
		/// </summary>
		/// <param name="query">to match documents
		/// </param>
		/// <param name="filter">if non-null, used to permit documents to be collected.
		/// </param>
		/// <param name="results">to receive hits
		/// </param>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		public virtual void  Search(Query query, Filter filter, Collector results)
		{
			Search(CreateWeight(query), filter, results);
		}
		
		/// <summary>Finds the top <code>n</code>
		/// hits for <code>query</code>, applying <code>filter</code> if non-null.
		/// 
		/// </summary>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		public virtual TopDocs Search(Query query, Filter filter, int n)
		{
			return Search(CreateWeight(query), filter, n);
		}
		
		/// <summary>Finds the top <code>n</code>
		/// hits for <code>query</code>.
		/// 
		/// </summary>
		/// <throws>  BooleanQuery.TooManyClauses </throws>
		public virtual TopDocs Search(Query query, int n)
		{
			return Search(query, null, n);
		}
		
		/// <summary>Returns an Explanation that describes how <code>doc</code> scored against
		/// <code>query</code>.
		/// 
		/// <p/>This is intended to be used in developing Similarity implementations,
		/// and, for good performance, should not be displayed with every hit.
		/// Computing an explanation is as expensive as executing the query over the
		/// entire index.
		/// </summary>
		public virtual Explanation Explain(Query query, int doc)
		{
			return Explain(CreateWeight(query), doc);
		}
		
		/// <summary>The Similarity implementation used by this searcher. </summary>
		private Similarity similarity;
		
		/// <summary>Expert: Set the Similarity implementation used by this Searcher.
		/// 
		/// </summary>
		/// <seealso cref="Similarity.SetDefault(Similarity)">
		/// </seealso>
		public virtual void  SetSimilarity(Similarity similarity)
		{
			this.similarity = similarity;
		}
		
		/// <summary>Expert: Return the Similarity implementation used by this Searcher.
		/// 
		/// <p/>This defaults to the current value of {@link Similarity#GetDefault()}.
		/// </summary>
		public virtual Similarity GetSimilarity()
		{
			return this.similarity;
		}
		
		/// <summary> creates a weight for <code>query</code></summary>
		/// <returns> new weight
		/// </returns>
		public /*protected internal*/ virtual Weight CreateWeight(Query query)
		{
			return query.Weight(this);
		}
		
		// inherit javadoc
		public virtual int[] DocFreqs(Term[] terms)
		{
			int[] result = new int[terms.Length];
			for (int i = 0; i < terms.Length; i++)
			{
				result[i] = DocFreq(terms[i]);
			}
			return result;
		}
		
		/* The following abstract methods were added as a workaround for GCJ bug #15411.
		* http://gcc.gnu.org/bugzilla/show_bug.cgi?id=15411
		*/
		/// <deprecated> use {@link #Search(Weight, Filter, Collector)} instead.
		/// </deprecated>
        [Obsolete("use Search(Weight, Filter, Collector) instead.")]
		public virtual void  Search(Weight weight, Filter filter, HitCollector results)
		{
			Search(weight, filter, new HitCollectorWrapper(results));
		}
		abstract public void  Search(Weight weight, Filter filter, Collector results);
		abstract public void  Close();
        abstract public void Dispose();
		abstract public int DocFreq(Term term);
		abstract public int MaxDoc();
		abstract public TopDocs Search(Weight weight, Filter filter, int n);
		abstract public Document Doc(int i);
		abstract public Query Rewrite(Query query);
		abstract public Explanation Explain(Weight weight, int doc);
		abstract public TopFieldDocs Search(Weight weight, Filter filter, int n, Sort sort);
		/* End patch for GCJ bug #15411. */
		public abstract Mono.Lucene.Net.Documents.Document Doc(int param1, Mono.Lucene.Net.Documents.FieldSelector param2);
	}
}
