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
	
	/// <summary>The abstract base class for queries.
	/// <p/>Instantiable subclasses are:
	/// <ul>
	/// <li> {@link TermQuery} </li>
    /// <li> {@link MultiTermQuery} </li>
    /// <li> {@link BooleanQuery} </li>
    /// <li> {@link WildcardQuery} </li>
    /// <li> {@link PhraseQuery} </li>
    /// <li> {@link PrefixQuery} </li>
    /// <li> {@link MultiPhraseQuery} </li>
    /// <li> {@link FuzzyQuery} </li>
    /// <li> {@link TermRangeQuery} </li>
    /// <li> {@link NumericRangeQuery} </li>
    /// <li> {@link Mono.Lucene.Net.Search.Spans.SpanQuery} </li>
	/// </ul>
	/// <p/>A parser for queries is contained in:
	/// <ul>
    /// <li>{@link Mono.Lucene.Net.QueryParsers.QueryParser QueryParser} </li>
	/// </ul>
	/// </summary>
	[Serializable]
	public abstract class Query : System.ICloneable
	{
		private float boost = 1.0f; // query boost factor
		
		/// <summary>Sets the boost for this query clause to <code>b</code>.  Documents
		/// matching this clause will (in addition to the normal weightings) have
		/// their score multiplied by <code>b</code>.
		/// </summary>
		public virtual void  SetBoost(float b)
		{
			boost = b;
		}
		
		/// <summary>Gets the boost for this clause.  Documents matching
		/// this clause will (in addition to the normal weightings) have their score
		/// multiplied by <code>b</code>.   The boost is 1.0 by default.
		/// </summary>
		public virtual float GetBoost()
		{
			return boost;
		}
		
		/// <summary>Prints a query to a string, with <code>field</code> assumed to be the 
		/// default field and omitted.
		/// <p/>The representation used is one that is supposed to be readable
		/// by {@link Mono.Lucene.Net.QueryParsers.QueryParser QueryParser}. However,
		/// there are the following limitations:
		/// <ul>
		/// <li>If the query was created by the parser, the printed
		/// representation may not be exactly what was parsed. For example,
		/// characters that need to be escaped will be represented without
		/// the required backslash.</li>
		/// <li>Some of the more complicated queries (e.g. span queries)
		/// don't have a representation that can be parsed by QueryParser.</li>
		/// </ul>
		/// </summary>
		public abstract System.String ToString(System.String field);
		
		/// <summary>Prints a query to a string. </summary>
		public override System.String ToString()
		{
			return ToString("");
		}
		
		/// <summary> Expert: Constructs an appropriate Weight implementation for this query.
		/// 
		/// <p/>
		/// Only implemented by primitive queries, which re-write to themselves.
		/// </summary>
		public virtual Weight CreateWeight(Searcher searcher)
		{
			throw new System.NotSupportedException();
		}
		
		/// <summary> Expert: Constructs and initializes a Weight for a top-level query.</summary>
		public virtual Weight Weight(Searcher searcher)
		{
			Query query = searcher.Rewrite(this);
			Weight weight = query.CreateWeight(searcher);
			float sum = weight.SumOfSquaredWeights();
			float norm = GetSimilarity(searcher).QueryNorm(sum);
            if (float.IsInfinity(norm) || float.IsNaN(norm))
                norm = 1.0f;
			weight.Normalize(norm);
			return weight;
		}
		
		
		/// <summary>Expert: called to re-write queries into primitive queries. For example,
		/// a PrefixQuery will be rewritten into a BooleanQuery that consists
		/// of TermQuerys.
		/// </summary>
		public virtual Query Rewrite(IndexReader reader)
		{
			return this;
		}
		
		
		/// <summary>Expert: called when re-writing queries under MultiSearcher.
		/// 
		/// Create a single query suitable for use by all subsearchers (in 1-1
		/// correspondence with queries). This is an optimization of the OR of
		/// all queries. We handle the common optimization cases of equal
		/// queries and overlapping clauses of boolean OR queries (as generated
		/// by MultiTermQuery.rewrite()).
		/// Be careful overriding this method as queries[0] determines which
		/// method will be called and is not necessarily of the same type as
		/// the other queries.
		/// </summary>
		public virtual Query Combine(Query[] queries)
		{
            System.Collections.Hashtable uniques = new System.Collections.Hashtable();
			for (int i = 0; i < queries.Length; i++)
			{
				Query query = queries[i];
				BooleanClause[] clauses = null;
				// check if we can split the query into clauses
				bool splittable = (query is BooleanQuery);
				if (splittable)
				{
					BooleanQuery bq = (BooleanQuery) query;
					splittable = bq.IsCoordDisabled();
					clauses = bq.GetClauses();
					for (int j = 0; splittable && j < clauses.Length; j++)
					{
						splittable = (clauses[j].GetOccur() == BooleanClause.Occur.SHOULD);
					}
				}
				if (splittable)
				{
					for (int j = 0; j < clauses.Length; j++)
					{
						SupportClass.CollectionsHelper.AddIfNotContains(uniques, clauses[j].GetQuery());
					}
				}
				else
				{
					SupportClass.CollectionsHelper.AddIfNotContains(uniques, query);
				}
			}
			// optimization: if we have just one query, just return it
			if (uniques.Count == 1)
			{
                foreach (object key in uniques.Keys)
                {
                    return (Query) key;
                }
			}
			BooleanQuery result = new BooleanQuery(true);
            foreach (object key in uniques.Keys)
            {
                result.Add((Query) key, BooleanClause.Occur.SHOULD);
            }
			return result;
		}
		
		
		/// <summary> Expert: adds all terms occuring in this query to the terms set. Only
		/// works if this query is in its {@link #rewrite rewritten} form.
		/// 
		/// </summary>
		/// <throws>  UnsupportedOperationException if this query is not yet rewritten </throws>
		public virtual void  ExtractTerms(System.Collections.Hashtable terms)
		{
			// needs to be implemented by query subclasses
			throw new System.NotSupportedException();
		}
		
		
		
		/// <summary>Expert: merges the clauses of a set of BooleanQuery's into a single
		/// BooleanQuery.
		/// 
		/// <p/>A utility for use by {@link #Combine(Query[])} implementations.
		/// </summary>
		public static Query MergeBooleanQueries(BooleanQuery[] queries)
		{
            System.Collections.Hashtable allClauses = new System.Collections.Hashtable();
			for (int i = 0; i < queries.Length; i++)
			{
				BooleanClause[] clauses = queries[i].GetClauses();
				for (int j = 0; j < clauses.Length; j++)
				{
					SupportClass.CollectionsHelper.AddIfNotContains(allClauses, clauses[j]);
				}
			}
			
			bool coordDisabled = queries.Length == 0?false:queries[0].IsCoordDisabled();
			BooleanQuery result = new BooleanQuery(coordDisabled);
			System.Collections.IEnumerator i2 = allClauses.GetEnumerator();
			while (i2.MoveNext())
			{
				result.Add((BooleanClause) i2.Current);
			}
			return result;
		}
		
		
		/// <summary>Expert: Returns the Similarity implementation to be used for this query.
		/// Subclasses may override this method to specify their own Similarity
		/// implementation, perhaps one that delegates through that of the Searcher.
		/// By default the Searcher's Similarity implementation is returned.
		/// </summary>
		public virtual Similarity GetSimilarity(Searcher searcher)
		{
			return searcher.GetSimilarity();
		}
		
		/// <summary>Returns a clone of this query. </summary>
		public virtual System.Object Clone()
		{
			try
			{
				return base.MemberwiseClone();
			}
			catch (System.Exception e)
			{
				throw new System.SystemException("Clone not supported: " + e.Message);
			}
		}
		
		public override int GetHashCode()
		{
			int prime = 31;
			int result = 1;
			result = prime * result + BitConverter.ToInt32(BitConverter.GetBytes(boost), 0);
			return result;
		}
		
		public  override bool Equals(System.Object obj)
		{
			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (GetType() != obj.GetType())
				return false;
			Query other = (Query) obj;
			if (BitConverter.ToInt32(BitConverter.GetBytes(boost), 0) != BitConverter.ToInt32(BitConverter.GetBytes(other.boost), 0))
				return false;
			return true;
		}
	}
}
