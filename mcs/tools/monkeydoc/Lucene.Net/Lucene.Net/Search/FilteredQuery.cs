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
using ToStringUtils = Mono.Lucene.Net.Util.ToStringUtils;

namespace Mono.Lucene.Net.Search
{
	
	
	/// <summary> A query that applies a filter to the results of another query.
	/// 
	/// <p/>Note: the bits are retrieved from the filter each time this
	/// query is used in a search - use a CachingWrapperFilter to avoid
	/// regenerating the bits every time.
	/// 
	/// <p/>Created: Apr 20, 2004 8:58:29 AM
	/// 
	/// </summary>
	/// <since>   1.4
	/// </since>
	/// <version>  $Id: FilteredQuery.java 807821 2009-08-25 21:55:49Z mikemccand $
	/// </version>
	/// <seealso cref="CachingWrapperFilter">
	/// </seealso>
	[Serializable]
	public class FilteredQuery:Query
	{
		[Serializable]
		private class AnonymousClassWeight:Weight
		{
			public AnonymousClassWeight(Mono.Lucene.Net.Search.Weight weight, Mono.Lucene.Net.Search.Similarity similarity, FilteredQuery enclosingInstance)
			{
				InitBlock(weight, similarity, enclosingInstance);
			}
			private class AnonymousClassScorer:Scorer
			{
				private void  InitBlock(Mono.Lucene.Net.Search.Scorer scorer, Mono.Lucene.Net.Search.DocIdSetIterator docIdSetIterator, AnonymousClassWeight enclosingInstance)
				{
					this.scorer = scorer;
					this.docIdSetIterator = docIdSetIterator;
					this.enclosingInstance = enclosingInstance;
				}
				private Mono.Lucene.Net.Search.Scorer scorer;
				private Mono.Lucene.Net.Search.DocIdSetIterator docIdSetIterator;
				private AnonymousClassWeight enclosingInstance;
				public AnonymousClassWeight Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				internal AnonymousClassScorer(Mono.Lucene.Net.Search.Scorer scorer, Mono.Lucene.Net.Search.DocIdSetIterator docIdSetIterator, AnonymousClassWeight enclosingInstance, Mono.Lucene.Net.Search.Similarity Param1):base(Param1)
				{
					InitBlock(scorer, docIdSetIterator, enclosingInstance);
				}
				
				private int doc = - 1;
				
				private int AdvanceToCommon(int scorerDoc, int disiDoc)
				{
					while (scorerDoc != disiDoc)
					{
						if (scorerDoc < disiDoc)
						{
							scorerDoc = scorer.Advance(disiDoc);
						}
						else
						{
							disiDoc = docIdSetIterator.Advance(scorerDoc);
						}
					}
					return scorerDoc;
				}
				
				/// <deprecated> use {@link #NextDoc()} instead. 
				/// </deprecated>
                [Obsolete("use NextDoc() instead. ")]
				public override bool Next()
				{
					return NextDoc() != NO_MORE_DOCS;
				}
				
				public override int NextDoc()
				{
					int scorerDoc, disiDoc;
					return doc = (disiDoc = docIdSetIterator.NextDoc()) != NO_MORE_DOCS && (scorerDoc = scorer.NextDoc()) != NO_MORE_DOCS && AdvanceToCommon(scorerDoc, disiDoc) != NO_MORE_DOCS?scorer.DocID():NO_MORE_DOCS;
				}
				
				/// <deprecated> use {@link #DocID()} instead. 
				/// </deprecated>
                [Obsolete("use DocID() instead.")]
				public override int Doc()
				{
					return scorer.Doc();
				}
				public override int DocID()
				{
					return doc;
				}
				
				/// <deprecated> use {@link #Advance(int)} instead. 
				/// </deprecated>
                [Obsolete("use Advance(int) instead.")]
				public override bool SkipTo(int i)
				{
					return Advance(i) != NO_MORE_DOCS;
				}
				
				public override int Advance(int target)
				{
					int disiDoc, scorerDoc;
					return doc = (disiDoc = docIdSetIterator.Advance(target)) != NO_MORE_DOCS && (scorerDoc = scorer.Advance(disiDoc)) != NO_MORE_DOCS && AdvanceToCommon(scorerDoc, disiDoc) != NO_MORE_DOCS?scorer.DocID():NO_MORE_DOCS;
				}
				
				public override float Score()
				{
					return Enclosing_Instance.Enclosing_Instance.GetBoost() * scorer.Score();
				}
				
				// add an explanation about whether the document was filtered
				public override Explanation Explain(int i)
				{
					Explanation exp = scorer.Explain(i);
					
					if (docIdSetIterator.Advance(i) == i)
					{
						exp.SetDescription("allowed by filter: " + exp.GetDescription());
						exp.SetValue(Enclosing_Instance.Enclosing_Instance.GetBoost() * exp.GetValue());
					}
					else
					{
						exp.SetDescription("removed by filter: " + exp.GetDescription());
						exp.SetValue(0.0f);
					}
					return exp;
				}
			}
			private void  InitBlock(Mono.Lucene.Net.Search.Weight weight, Mono.Lucene.Net.Search.Similarity similarity, FilteredQuery enclosingInstance)
			{
				this.weight = weight;
				this.similarity = similarity;
				this.enclosingInstance = enclosingInstance;
			}
			private Mono.Lucene.Net.Search.Weight weight;
			private Mono.Lucene.Net.Search.Similarity similarity;
			private FilteredQuery enclosingInstance;
			public FilteredQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private float value_Renamed;
			
			// pass these methods through to enclosed query's weight
			public override float GetValue()
			{
				return value_Renamed;
			}
			public override float SumOfSquaredWeights()
			{
				return weight.SumOfSquaredWeights() * Enclosing_Instance.GetBoost() * Enclosing_Instance.GetBoost();
			}
			public override void  Normalize(float v)
			{
				weight.Normalize(v);
				value_Renamed = weight.GetValue() * Enclosing_Instance.GetBoost();
			}
			public override Explanation Explain(IndexReader ir, int i)
			{
				Explanation inner = weight.Explain(ir, i);
				if (Enclosing_Instance.GetBoost() != 1)
				{
					Explanation preBoost = inner;
					inner = new Explanation(inner.GetValue() * Enclosing_Instance.GetBoost(), "product of:");
					inner.AddDetail(new Explanation(Enclosing_Instance.GetBoost(), "boost"));
					inner.AddDetail(preBoost);
				}
				Filter f = Enclosing_Instance.filter;
				DocIdSet docIdSet = f.GetDocIdSet(ir);
				DocIdSetIterator docIdSetIterator = docIdSet == null?DocIdSet.EMPTY_DOCIDSET.Iterator():docIdSet.Iterator();
				if (docIdSetIterator == null)
				{
					docIdSetIterator = DocIdSet.EMPTY_DOCIDSET.Iterator();
				}
				if (docIdSetIterator.Advance(i) == i)
				{
					return inner;
				}
				else
				{
					Explanation result = new Explanation(0.0f, "failure to match filter: " + f.ToString());
					result.AddDetail(inner);
					return result;
				}
			}
			
			// return this query
			public override Query GetQuery()
			{
				return Enclosing_Instance;
			}
			
			// return a filtering scorer
			public override Scorer Scorer(IndexReader indexReader, bool scoreDocsInOrder, bool topScorer)
			{
				Scorer scorer = weight.Scorer(indexReader, true, false);
				if (scorer == null)
				{
					return null;
				}
				DocIdSet docIdSet = Enclosing_Instance.filter.GetDocIdSet(indexReader);
				if (docIdSet == null)
				{
					return null;
				}
				DocIdSetIterator docIdSetIterator = docIdSet.Iterator();
				if (docIdSetIterator == null)
				{
					return null;
				}
				
				return new AnonymousClassScorer(scorer, docIdSetIterator, this, similarity);
			}
		}
		
		internal Query query;
		internal Filter filter;
		
		/// <summary> Constructs a new query which applies a filter to the results of the original query.
		/// Filter.getDocIdSet() will be called every time this query is used in a search.
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
		public override Weight CreateWeight(Searcher searcher)
		{
			Weight weight = query.CreateWeight(searcher);
			Similarity similarity = query.GetSimilarity(searcher);
			return new AnonymousClassWeight(weight, similarity, this);
		}
		
		/// <summary>Rewrites the wrapped query. </summary>
		public override Query Rewrite(IndexReader reader)
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
		
		public virtual Filter GetFilter()
		{
			return filter;
		}
		
		// inherit javadoc
		public override void  ExtractTerms(System.Collections.Hashtable terms)
		{
			GetQuery().ExtractTerms(terms);
		}
		
		/// <summary>Prints a user-readable version of this query. </summary>
		public override System.String ToString(System.String s)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("filtered(");
			buffer.Append(query.ToString(s));
			buffer.Append(")->");
			buffer.Append(filter);
			buffer.Append(ToStringUtils.Boost(GetBoost()));
			return buffer.ToString();
		}
		
		/// <summary>Returns true iff <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (o is FilteredQuery)
			{
				FilteredQuery fq = (FilteredQuery) o;
				return (query.Equals(fq.query) && filter.Equals(fq.filter) && GetBoost() == fq.GetBoost());
			}
			return false;
		}
		
		/// <summary>Returns a hash code value for this object. </summary>
		public override int GetHashCode()
		{
			return query.GetHashCode() ^ filter.GetHashCode() + System.Convert.ToInt32(GetBoost());
		}
	}
}
