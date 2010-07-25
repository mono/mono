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
	
	/// <summary>The abstract base class for queries.
	/// <p>Instantiable subclasses are:
	/// <ul>
	/// <li> {@link TermQuery}
	/// <li> {@link MultiTermQuery}
	/// <li> {@link BooleanQuery}
	/// <li> {@link WildcardQuery}
	/// <li> {@link PhraseQuery}
	/// <li> {@link PrefixQuery}
	/// <li> {@link PhrasePrefixQuery}
	/// <li> {@link FuzzyQuery}
	/// <li> {@link RangeQuery}
	/// <li> {@link Monodoc.Lucene.Net.Search.Spans.SpanQuery}
	/// </ul>
	/// <p>A parser for queries is contained in:
	/// <ul>
	/// <li>{@link Monodoc.Lucene.Net.QueryParser.QueryParser QueryParser}
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
		
		/// <summary>Prints a query to a string, with <code>Field</code> as the default Field
		/// for terms.  <p>The representation used is one that is readable by
		/// {@link Monodoc.Lucene.Net.QueryParser.QueryParser QueryParser}
		/// (although, if the query was created by the parser, the printed
		/// representation may not be exactly what was parsed).
		/// </summary>
		public abstract System.String ToString(System.String field);
		
		/// <summary>Prints a query to a string. </summary>
		public override System.String ToString()
		{
			return ToString("");
		}
		
		/// <summary>Expert: Constructs an appropriate Weight implementation for this query.
		/// 
		/// <p>Only implemented by primitive queries, which re-write to themselves.
		/// </summary>
		protected internal virtual Weight CreateWeight(Searcher searcher)
		{
			throw new System.NotSupportedException();
		}
		
		/// <summary>Expert: Constructs an initializes a Weight for a top-level query. </summary>
		public virtual Weight Weight(Searcher searcher)
		{
			Query query = searcher.Rewrite(this);
			Weight weight = query.CreateWeight(searcher);
			float sum = weight.SumOfSquaredWeights();
			float norm = GetSimilarity(searcher).QueryNorm(sum);
			weight.Normalize(norm);
			return weight;
		}
		
		/// <summary>Expert: called to re-write queries into primitive queries. </summary>
		public virtual Query Rewrite(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			return this;
		}
		
		/// <summary>Expert: called when re-writing queries under MultiSearcher.
		/// 
		/// <p>Only implemented by derived queries, with no
		/// {@link #CreateWeight(Searcher)} implementatation.
		/// </summary>
		public virtual Query Combine(Query[] queries)
		{
			throw new System.NotSupportedException();
		}
		
		
		/// <summary>Expert: merges the clauses of a set of BooleanQuery's into a single
		/// BooleanQuery.
		/// 
		/// <p>A utility for use by {@link #Combine(Query[])} implementations.
		/// </summary>
		public static Query MergeBooleanQueries(Query[] queries)
		{
			System.Collections.Hashtable allClauses = new System.Collections.Hashtable();
			for (int i = 0; i < queries.Length; i++)
			{
				BooleanClause[] clauses = ((BooleanQuery) queries[i]).GetClauses();
				for (int j = 0; j < clauses.Length; j++)
				{
					allClauses.Add(clauses[j], clauses[j]);
				}
			}
			
            BooleanQuery result = new BooleanQuery();
            foreach (BooleanClause booleanClause in allClauses.Keys)
            {
                result.Add(booleanClause);
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
				return (Query) this.MemberwiseClone();
			}
			catch (System.Exception e)
			{
				throw new System.SystemException("Clone not supported: " + e.Message);
			}
		}
	}
}