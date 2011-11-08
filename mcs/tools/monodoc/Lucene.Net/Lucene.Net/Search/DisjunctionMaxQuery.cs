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
	
	/// <summary> A query that generates the union of documents produced by its subqueries, and that scores each document with the maximum
	/// score for that document as produced by any subquery, plus a tie breaking increment for any additional matching subqueries.
	/// This is useful when searching for a word in multiple fields with different boost factors (so that the fields cannot be
	/// combined equivalently into a single search field).  We want the primary score to be the one associated with the highest boost,
	/// not the sum of the field scores (as BooleanQuery would give).
	/// If the query is "albino elephant" this ensures that "albino" matching one field and "elephant" matching
	/// another gets a higher score than "albino" matching both fields.
	/// To get this result, use both BooleanQuery and DisjunctionMaxQuery:  for each term a DisjunctionMaxQuery searches for it in
	/// each field, while the set of these DisjunctionMaxQuery's is combined into a BooleanQuery.
	/// The tie breaker capability allows results that include the same term in multiple fields to be judged better than results that
	/// include this term in only the best of those multiple fields, without confusing this with the better case of two different terms
	/// in the multiple fields.
	/// </summary>
	[Serializable]
	public class DisjunctionMaxQuery:Query, System.ICloneable
	{
		
		/* The subqueries */
		private SupportClass.EquatableList<Query> disjuncts = new SupportClass.EquatableList<Query>();
		
		/* Multiple of the non-max disjunct scores added into our final score.  Non-zero values support tie-breaking. */
		private float tieBreakerMultiplier = 0.0f;
		
		/// <summary>Creates a new empty DisjunctionMaxQuery.  Use add() to add the subqueries.</summary>
		/// <param name="tieBreakerMultiplier">the score of each non-maximum disjunct for a document is multiplied by this weight
		/// and added into the final score.  If non-zero, the value should be small, on the order of 0.1, which says that
		/// 10 occurrences of word in a lower-scored field that is also in a higher scored field is just as good as a unique
		/// word in the lower scored field (i.e., one that is not in any higher scored field.
		/// </param>
		public DisjunctionMaxQuery(float tieBreakerMultiplier)
		{
			this.tieBreakerMultiplier = tieBreakerMultiplier;
		}
		
		/// <summary> Creates a new DisjunctionMaxQuery</summary>
        /// <param name="disjuncts">a Collection&lt;Query&gt; of all the disjuncts to add
		/// </param>
		/// <param name="tieBreakerMultiplier">  the weight to give to each matching non-maximum disjunct
		/// </param>
		public DisjunctionMaxQuery(System.Collections.ICollection disjuncts, float tieBreakerMultiplier)
		{
			this.tieBreakerMultiplier = tieBreakerMultiplier;
			Add(disjuncts);
		}
		
		/// <summary>Add a subquery to this disjunction</summary>
		/// <param name="query">the disjunct added
		/// </param>
		public virtual void  Add(Query query)
		{
			disjuncts.Add(query);
		}
		
		/// <summary>Add a collection of disjuncts to this disjunction
		/// via Iterable
		/// </summary>
		public virtual void  Add(System.Collections.ICollection disjuncts)
		{
			this.disjuncts.AddRange(disjuncts);
		}

        /// <summary>An Iterator&lt;Query&gt; over the disjuncts </summary>
		public virtual System.Collections.IEnumerator Iterator()
		{
			return disjuncts.GetEnumerator();
		}
		
		/// <summary> Expert: the Weight for DisjunctionMaxQuery, used to
		/// normalize, score and explain these queries.
		/// 
		/// <p/>NOTE: this API and implementation is subject to
		/// change suddenly in the next release.<p/>
		/// </summary>
		[Serializable]
		protected internal class DisjunctionMaxWeight:Weight
		{
			private void  InitBlock(DisjunctionMaxQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private DisjunctionMaxQuery enclosingInstance;
			public DisjunctionMaxQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			/// <summary>The Similarity implementation. </summary>
			protected internal Similarity similarity;
			
			/// <summary>The Weights for our subqueries, in 1-1 correspondence with disjuncts </summary>
			protected internal System.Collections.ArrayList weights = new System.Collections.ArrayList(); // The Weight's for our subqueries, in 1-1 correspondence with disjuncts
			
			/* Construct the Weight for this Query searched by searcher.  Recursively construct subquery weights. */
			public DisjunctionMaxWeight(DisjunctionMaxQuery enclosingInstance, Searcher searcher)
			{
				InitBlock(enclosingInstance);
				this.similarity = searcher.GetSimilarity();
				for (System.Collections.IEnumerator iter = Enclosing_Instance.disjuncts.GetEnumerator(); iter.MoveNext(); )
				{
					weights.Add(((Query) iter.Current).CreateWeight(searcher));
				}
			}
			
			/* Return our associated DisjunctionMaxQuery */
			public override Query GetQuery()
			{
				return Enclosing_Instance;
			}
			
			/* Return our boost */
			public override float GetValue()
			{
				return Enclosing_Instance.GetBoost();
			}
			
			/* Compute the sub of squared weights of us applied to our subqueries.  Used for normalization. */
			public override float SumOfSquaredWeights()
			{
				float max = 0.0f, sum = 0.0f;
				for (System.Collections.IEnumerator iter = weights.GetEnumerator(); iter.MoveNext(); )
				{
					float sub = ((Weight) iter.Current).SumOfSquaredWeights();
					sum += sub;
					max = System.Math.Max(max, sub);
				}
				float boost = Enclosing_Instance.GetBoost();
				return (((sum - max) * Enclosing_Instance.tieBreakerMultiplier * Enclosing_Instance.tieBreakerMultiplier) + max) * boost * boost;
			}
			
			/* Apply the computed normalization factor to our subqueries */
			public override void  Normalize(float norm)
			{
				norm *= Enclosing_Instance.GetBoost(); // Incorporate our boost
				for (System.Collections.IEnumerator iter = weights.GetEnumerator(); iter.MoveNext(); )
				{
					((Weight) iter.Current).Normalize(norm);
				}
			}
			
			/* Create the scorer used to score our associated DisjunctionMaxQuery */
			public override Scorer Scorer(IndexReader reader, bool scoreDocsInOrder, bool topScorer)
			{
				Scorer[] scorers = new Scorer[weights.Count];
				int idx = 0;
				for (System.Collections.IEnumerator iter = weights.GetEnumerator(); iter.MoveNext(); )
				{
					Weight w = (Weight) iter.Current;
					Scorer subScorer = w.Scorer(reader, true, false);
					if (subScorer != null && subScorer.NextDoc() != DocIdSetIterator.NO_MORE_DOCS)
					{
						scorers[idx++] = subScorer;
					}
				}
				if (idx == 0)
					return null; // all scorers did not have documents
				DisjunctionMaxScorer result = new DisjunctionMaxScorer(Enclosing_Instance.tieBreakerMultiplier, similarity, scorers, idx);
				return result;
			}
			
			/* Explain the score we computed for doc */
			public override Explanation Explain(IndexReader reader, int doc)
			{
				if (Enclosing_Instance.disjuncts.Count == 1)
					return ((Weight) weights[0]).Explain(reader, doc);
				ComplexExplanation result = new ComplexExplanation();
				float max = 0.0f, sum = 0.0f;
				result.SetDescription(Enclosing_Instance.tieBreakerMultiplier == 0.0f?"max of:":"max plus " + Enclosing_Instance.tieBreakerMultiplier + " times others of:");
				for (System.Collections.IEnumerator iter = weights.GetEnumerator(); iter.MoveNext(); )
				{
					Explanation e = ((Weight) iter.Current).Explain(reader, doc);
					if (e.IsMatch())
					{
						System.Boolean tempAux = true;
						result.SetMatch(tempAux);
						result.AddDetail(e);
						sum += e.GetValue();
						max = System.Math.Max(max, e.GetValue());
					}
				}
				result.SetValue(max + (sum - max) * Enclosing_Instance.tieBreakerMultiplier);
				return result;
			}
		} // end of DisjunctionMaxWeight inner class
		
		/* Create the Weight used to score us */
		public override Weight CreateWeight(Searcher searcher)
		{
			return new DisjunctionMaxWeight(this, searcher);
		}
		
		/// <summary>Optimize our representation and our subqueries representations</summary>
		/// <param name="reader">the IndexReader we query
		/// </param>
		/// <returns> an optimized copy of us (which may not be a copy if there is nothing to optimize) 
		/// </returns>
		public override Query Rewrite(IndexReader reader)
		{
			int numDisjunctions = disjuncts.Count;
			if (numDisjunctions == 1)
			{
				Query singleton = (Query) disjuncts[0];
				Query result = singleton.Rewrite(reader);
				if (GetBoost() != 1.0f)
				{
					if (result == singleton)
						result = (Query) result.Clone();
					result.SetBoost(GetBoost() * result.GetBoost());
				}
				return result;
			}
			DisjunctionMaxQuery clone = null;
			for (int i = 0; i < numDisjunctions; i++)
			{
				Query clause = (Query) disjuncts[i];
				Query rewrite = clause.Rewrite(reader);
				if (rewrite != clause)
				{
					if (clone == null)
						clone = (DisjunctionMaxQuery) this.Clone();
					clone.disjuncts[i] = rewrite;
				}
			}
			if (clone != null)
				return clone;
			else
				return this;
		}
		
		/// <summary>Create a shallow copy of us -- used in rewriting if necessary</summary>
		/// <returns> a copy of us (but reuse, don't copy, our subqueries) 
		/// </returns>
		public override System.Object Clone()
		{
			DisjunctionMaxQuery clone = (DisjunctionMaxQuery) base.Clone();
            clone.disjuncts = (SupportClass.EquatableList<Query>) this.disjuncts.Clone();
			return clone;
		}
		
		// inherit javadoc
		public override void  ExtractTerms(System.Collections.Hashtable terms)
		{
			for (System.Collections.IEnumerator iter = disjuncts.GetEnumerator(); iter.MoveNext(); )
			{
				((Query) iter.Current).ExtractTerms(terms);
			}
		}
		
		/// <summary>Prettyprint us.</summary>
		/// <param name="field">the field to which we are applied
		/// </param>
		/// <returns> a string that shows what we do, of the form "(disjunct1 | disjunct2 | ... | disjunctn)^boost"
		/// </returns>
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("(");
			int numDisjunctions = disjuncts.Count;
			for (int i = 0; i < numDisjunctions; i++)
			{
				Query subquery = (Query) disjuncts[i];
				if (subquery is BooleanQuery)
				{
					// wrap sub-bools in parens
					buffer.Append("(");
					buffer.Append(subquery.ToString(field));
					buffer.Append(")");
				}
				else
					buffer.Append(subquery.ToString(field));
				if (i != numDisjunctions - 1)
					buffer.Append(" | ");
			}
			buffer.Append(")");
			if (tieBreakerMultiplier != 0.0f)
			{
				buffer.Append("~");
				buffer.Append(tieBreakerMultiplier);
			}
			if (GetBoost() != 1.0)
			{
				buffer.Append("^");
				buffer.Append(GetBoost());
			}
			return buffer.ToString();
		}
		
		/// <summary>Return true iff we represent the same query as o</summary>
		/// <param name="o">another object
		/// </param>
		/// <returns> true iff o is a DisjunctionMaxQuery with the same boost and the same subqueries, in the same order, as us
		/// </returns>
		public  override bool Equals(System.Object o)
		{
			if (!(o is DisjunctionMaxQuery))
				return false;
			DisjunctionMaxQuery other = (DisjunctionMaxQuery) o;
			return this.GetBoost() == other.GetBoost() && this.tieBreakerMultiplier == other.tieBreakerMultiplier && this.disjuncts.Equals(other.disjuncts);
		}
		
		/// <summary>Compute a hash code for hashing us</summary>
		/// <returns> the hash code
		/// </returns>
		public override int GetHashCode()
		{
			return BitConverter.ToInt32(BitConverter.GetBytes(GetBoost()), 0) + BitConverter.ToInt32(BitConverter.GetBytes(tieBreakerMultiplier), 0) + disjuncts.GetHashCode();
		}
	}
}
