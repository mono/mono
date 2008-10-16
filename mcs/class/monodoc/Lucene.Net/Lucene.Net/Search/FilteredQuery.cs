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
using Monodoc.Lucene.Net.Index;
namespace Monodoc.Lucene.Net.Search
{
	
	
	/// <summary> A query that applies a filter to the results of another query.
	/// 
	/// <p>Note: the bits are retrieved from the filter each time this
	/// query is used in a search - use a CachingWrapperFilter to avoid
	/// regenerating the bits every time.
	/// 
	/// <p>Created: Apr 20, 2004 8:58:29 AM
	/// 
	/// </summary>
	/// <author>   Tim Jones
	/// </author>
	/// <since>   1.4
	/// </since>
	/// <version>  $Id: FilteredQuery.java,v 1.5 2004/06/18 09:52:25 ehatcher Exp $
	/// </version>
	/// <seealso cref="CachingWrapperFilter">
	/// </seealso>
	[Serializable]
	public class FilteredQuery : Query
	{
		[Serializable]
		private class AnonymousClassWeight : Weight
		{
			public AnonymousClassWeight(Monodoc.Lucene.Net.Search.Weight weight, Monodoc.Lucene.Net.Search.Searcher searcher, FilteredQuery enclosingInstance)
			{
				InitBlock(weight, searcher, enclosingInstance);
			}
			private class AnonymousClassScorer : Scorer
			{
				private void  InitBlock(Monodoc.Lucene.Net.Search.Scorer scorer, System.Collections.BitArray bitset, AnonymousClassWeight enclosingInstance)
				{
					this.scorer = scorer;
					this.bitset = bitset;
					this.enclosingInstance = enclosingInstance;
				}
				private Monodoc.Lucene.Net.Search.Scorer scorer;
				private System.Collections.BitArray bitset;
				private AnonymousClassWeight enclosingInstance;
				public AnonymousClassWeight Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				internal AnonymousClassScorer(Monodoc.Lucene.Net.Search.Scorer scorer, System.Collections.BitArray bitset, AnonymousClassWeight enclosingInstance, Monodoc.Lucene.Net.Search.Similarity Param1):base(Param1)
				{
					InitBlock(scorer, bitset, enclosingInstance);
				}
				
				// pass these methods through to the enclosed scorer
				public override bool Next()
				{
					return scorer.Next();
				}
				public override int Doc()
				{
					return scorer.Doc();
				}
				public override bool SkipTo(int i)
				{
					return scorer.SkipTo(i);
				}
				
				// if the document has been filtered out, set score to 0.0
				public override float Score()
				{
					return (bitset.Get(scorer.Doc()))?scorer.Score():0.0f;
				}
				
				// add an explanation about whether the document was filtered
				public override Explanation Explain(int i)
				{
					Explanation exp = scorer.Explain(i);
					if (bitset.Get(i))
						exp.SetDescription("allowed by filter: " + exp.GetDescription());
					else
						exp.SetDescription("removed by filter: " + exp.GetDescription());
					return exp;
				}
			}
			private void  InitBlock(Monodoc.Lucene.Net.Search.Weight weight, Monodoc.Lucene.Net.Search.Searcher searcher, FilteredQuery enclosingInstance)
			{
				this.weight = weight;
				this.searcher = searcher;
				this.enclosingInstance = enclosingInstance;
			}
			private Monodoc.Lucene.Net.Search.Weight weight;
			private Monodoc.Lucene.Net.Search.Searcher searcher;
			private FilteredQuery enclosingInstance;
            virtual public float Value
            {
                // pass these methods through to enclosed query's weight
				
                get
                {
                    return weight.Value;
                }
				
            }
            virtual public Query Query
            {
                // return this query
				
                get
                {
                    return Enclosing_Instance;
                }
				
            }
            public FilteredQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public virtual float SumOfSquaredWeights()
			{
				return weight.SumOfSquaredWeights();
			}
			public virtual void  Normalize(float v)
			{
				weight.Normalize(v);
			}
			public virtual Explanation Explain(Monodoc.Lucene.Net.Index.IndexReader ir, int i)
			{
				return weight.Explain(ir, i);
			}
			
			// return a scorer that overrides the enclosed query's score if
			// the given hit has been filtered out.
			public virtual Scorer Scorer(Monodoc.Lucene.Net.Index.IndexReader indexReader)
			{
				Scorer scorer = weight.Scorer(indexReader);
				System.Collections.BitArray bitset = Enclosing_Instance.filter.Bits(indexReader);
				return new AnonymousClassScorer(scorer, bitset, this, Enclosing_Instance.query.GetSimilarity(searcher));
			}
		}
		
		internal Query query;
		internal Filter filter;
		
		/// <summary> Constructs a new query which applies a filter to the results of the original query.
		/// Filter.bits() will be called every time this query is used in a search.
		/// </summary>
		/// <param name="query"> Query to be filtered, cannot be <code>null</code>.
		/// </param>
		/// <param name="filter">Filter to apply to query results, cannot be <code>null</code>.
		/// </param>
		public FilteredQuery(Query query, Filter filter)
		{
			this.query = query;
			this.filter = filter;
		}
		
		/// <summary> Returns a Weight that applies the filter to the enclosed query's Weight.
		/// This is accomplished by overriding the Scorer returned by the Weight.
		/// </summary>
		protected internal override Weight CreateWeight(Searcher searcher)
		{
			Weight weight = query.CreateWeight(searcher);
			return new AnonymousClassWeight(weight, searcher, this);
		}
		
		/// <summary>Rewrites the wrapped query. </summary>
		public override Query Rewrite(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			Query rewritten = query.Rewrite(reader);
			if (rewritten != query)
			{
				FilteredQuery clone = (FilteredQuery) this.Clone();
				clone.query = rewritten;
				return clone;
			}
			else
			{
				return this;
			}
		}
		
		public virtual Query GetQuery()
		{
			return query;
		}
		
		/// <summary>Prints a user-readable version of this query. </summary>
		public override System.String ToString(System.String s)
		{
			return "filtered(" + query.ToString(s) + ")->" + filter;
		}
		
		/// <summary>Returns true iff <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (o is FilteredQuery)
			{
				FilteredQuery fq = (FilteredQuery) o;
				return (query.Equals(fq.query) && filter.Equals(fq.filter));
			}
			return false;
		}
		
		/// <summary>Returns a hash code value for this object. </summary>
		public override int GetHashCode()
		{
			return query.GetHashCode() ^ filter.GetHashCode();
		}
	}
}