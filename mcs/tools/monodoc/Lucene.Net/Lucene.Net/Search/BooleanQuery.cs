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
	
	/// <summary>A Query that matches documents matching boolean combinations of other
	/// queries, typically {@link TermQuery}s or {@link PhraseQuery}s.
	/// </summary>
	[Serializable]
	public class BooleanQuery:Query, System.ICloneable
	{
		
		/// <summary> Default value is 1024.  Use <code>Monodoc.Lucene.Net.maxClauseCount</code>
		/// system property to override.
		/// </summary>
		public static int maxClauseCount = System.Int32.Parse(SupportClass.AppSettings.Get("Monodoc.Lucene.Net.maxClauseCount", "1024"));
		
		/// <summary>Thrown when an attempt is made to add more than {@link
		/// #GetMaxClauseCount()} clauses. 
		/// </summary>
		[Serializable]
		public class TooManyClauses:System.SystemException
		{
		}
		
		/// <summary>Return the maximum number of clauses permitted, 1024 by default.
		/// Attempts to add more than the permitted number of clauses cause {@link
		/// TooManyClauses} to be thrown.
		/// </summary>
		public static int GetMaxClauseCount()
		{
			return maxClauseCount;
		}
		
		/// <summary>Set the maximum number of clauses permitted. </summary>
		public static void  SetMaxClauseCount(int maxClauseCount)
		{
			BooleanQuery.maxClauseCount = maxClauseCount;
		}
		
		private System.Collections.ArrayList clauses = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
		
		/// <summary>Constructs an empty boolean query. </summary>
		public BooleanQuery()
		{
		}
		
		/// <summary>Adds a clause to a boolean query.  Clauses may be:
		/// <ul>
		/// <li><code>required</code> which means that documents which <i>do not</i>
		/// match this sub-query will <i>not</i> match the boolean query;
		/// <li><code>prohibited</code> which means that documents which <i>do</i>
		/// match this sub-query will <i>not</i> match the boolean query; or
		/// <li>neither, in which case matched documents are neither prohibited from
		/// nor required to match the sub-query. However, a document must match at
		/// least 1 sub-query to match the boolean query.
		/// </ul>
		/// It is an error to specify a clause as both <code>required</code> and
		/// <code>prohibited</code>.
		/// 
		/// </summary>
		/// <seealso cref="#GetMaxClauseCount()">
		/// </seealso>
		public virtual void  Add(Query query, bool required, bool prohibited)
		{
			Add(new BooleanClause(query, required, prohibited));
		}
		
		/// <summary>Adds a clause to a boolean query.</summary>
		/// <seealso cref="#GetMaxClauseCount()">
		/// </seealso>
		public virtual void  Add(BooleanClause clause)
		{
			if (clauses.Count >= maxClauseCount)
				throw new TooManyClauses();
			
			clauses.Add(clause);
		}
		
		/// <summary>Returns the set of clauses in this query. </summary>
		public virtual BooleanClause[] GetClauses()
		{
            return (BooleanClause[]) clauses.ToArray(typeof(BooleanClause));
		}
		
		[Serializable]
		private class BooleanWeight : Weight
		{
			private void  InitBlock(BooleanQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private BooleanQuery enclosingInstance;
            virtual public Query Query
            {
                get
                {
                    return Enclosing_Instance;
                }
				
            }
            virtual public float Value
            {
                get
                {
                    return Enclosing_Instance.GetBoost();
                }
				
            }
            public BooleanQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private Searcher searcher;
			private System.Collections.ArrayList weights = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
			
			public BooleanWeight(BooleanQuery enclosingInstance, Searcher searcher)
			{
				InitBlock(enclosingInstance);
				this.searcher = searcher;
				for (int i = 0; i < Enclosing_Instance.clauses.Count; i++)
				{
					BooleanClause c = (BooleanClause) Enclosing_Instance.clauses[i];
					weights.Add(c.query.CreateWeight(searcher));
				}
			}
			
			public virtual float SumOfSquaredWeights()
			{
				float sum = 0.0f;
				for (int i = 0; i < weights.Count; i++)
				{
					BooleanClause c = (BooleanClause) Enclosing_Instance.clauses[i];
					Weight w = (Weight) weights[i];
					if (!c.prohibited)
						sum += w.SumOfSquaredWeights(); // sum sub weights
				}
				
				sum *= Enclosing_Instance.GetBoost() * Enclosing_Instance.GetBoost(); // boost each sub-weight
				
				return sum;
			}
			
			
			public virtual void  Normalize(float norm)
			{
				norm *= Enclosing_Instance.GetBoost(); // incorporate boost
				for (int i = 0; i < weights.Count; i++)
				{
					BooleanClause c = (BooleanClause) Enclosing_Instance.clauses[i];
					Weight w = (Weight) weights[i];
					if (!c.prohibited)
						w.Normalize(norm);
				}
			}
			
			public virtual Scorer Scorer(Monodoc.Lucene.Net.Index.IndexReader reader)
			{
				// First see if the (faster) ConjunctionScorer will work.  This can be
				// used when all clauses are required.  Also, at this point a
				// BooleanScorer cannot be embedded in a ConjunctionScorer, as the hits
				// from a BooleanScorer are not always sorted by document number (sigh)
				// and hence BooleanScorer cannot implement skipTo() correctly, which is
				// required by ConjunctionScorer.
				bool allRequired = true;
				bool noneBoolean = true;
				for (int i = 0; i < weights.Count; i++)
				{
					BooleanClause c = (BooleanClause) Enclosing_Instance.clauses[i];
					if (!c.required)
						allRequired = false;
					if (c.query is BooleanQuery)
						noneBoolean = false;
				}
				
				if (allRequired && noneBoolean)
				{
					// ConjunctionScorer is okay
					ConjunctionScorer result = new ConjunctionScorer(Enclosing_Instance.GetSimilarity(searcher));
					for (int i = 0; i < weights.Count; i++)
					{
						Weight w = (Weight) weights[i];
						Scorer subScorer = w.Scorer(reader);
						if (subScorer == null)
							return null;
						result.Add(subScorer);
					}
					return result;
				}
				
				// Use good-old BooleanScorer instead.
				BooleanScorer result2 = new BooleanScorer(Enclosing_Instance.GetSimilarity(searcher));
				
				for (int i = 0; i < weights.Count; i++)
				{
					BooleanClause c = (BooleanClause) Enclosing_Instance.clauses[i];
					Weight w = (Weight) weights[i];
					Scorer subScorer = w.Scorer(reader);
					if (subScorer != null)
						result2.Add(subScorer, c.required, c.prohibited);
					else if (c.required)
						return null;
				}
				
				return result2;
			}
			
			public virtual Explanation Explain(Monodoc.Lucene.Net.Index.IndexReader reader, int doc)
			{
				Explanation sumExpl = new Explanation();
				sumExpl.SetDescription("sum of:");
				int coord = 0;
				int maxCoord = 0;
				float sum = 0.0f;
				for (int i = 0; i < weights.Count; i++)
				{
					BooleanClause c = (BooleanClause) Enclosing_Instance.clauses[i];
					Weight w = (Weight) weights[i];
					Explanation e = w.Explain(reader, doc);
					if (!c.prohibited)
						maxCoord++;
					if (e.GetValue() > 0)
					{
						if (!c.prohibited)
						{
							sumExpl.AddDetail(e);
							sum += e.GetValue();
							coord++;
						}
						else
						{
							return new Explanation(0.0f, "match prohibited");
						}
					}
					else if (c.required)
					{
						return new Explanation(0.0f, "match required");
					}
				}
				sumExpl.SetValue(sum);
				
				if (coord == 1)
				// only one clause matched
					sumExpl = sumExpl.GetDetails()[0]; // eliminate wrapper
				
				float coordFactor = Enclosing_Instance.GetSimilarity(searcher).Coord(coord, maxCoord);
				if (coordFactor == 1.0f)
				// coord is no-op
					return sumExpl;
				// eliminate wrapper
				else
				{
					Explanation result = new Explanation();
					result.SetDescription("product of:");
					result.AddDetail(sumExpl);
					result.AddDetail(new Explanation(coordFactor, "coord(" + coord + "/" + maxCoord + ")"));
					result.SetValue(sum * coordFactor);
					return result;
				}
			}
		}
		
		protected internal override Weight CreateWeight(Searcher searcher)
		{
			return new BooleanWeight(this, searcher);
		}
		
		public override Query Rewrite(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			if (clauses.Count == 1)
			{
				// optimize 1-clause queries
				BooleanClause c = (BooleanClause) clauses[0];
				if (!c.prohibited)
				{
					// just return clause
					
					Query query = c.query.Rewrite(reader); // rewrite first
					
					if (GetBoost() != 1.0f)
					{
						// incorporate boost
						if (query == c.query)
						// if rewrite was no-op
							query = (Query) query.Clone(); // then clone before boost
						query.SetBoost(GetBoost() * query.GetBoost());
					}
					
					return query;
				}
			}
			
			BooleanQuery clone = null; // recursively rewrite
			for (int i = 0; i < clauses.Count; i++)
			{
				BooleanClause c = (BooleanClause) clauses[i];
				Query query = c.query.Rewrite(reader);
				if (query != c.query)
				{
					// clause rewrote: must clone
					if (clone == null)
						clone = (BooleanQuery) this.Clone();
					clone.clauses[i] = new BooleanClause(query, c.required, c.prohibited);
				}
			}
			if (clone != null)
			{
				return clone; // some clauses rewrote
			}
			else
				return this; // no clauses rewrote
		}
		
		
		public override System.Object Clone()
		{
			BooleanQuery clone = (BooleanQuery) base.Clone();
			clone.clauses = (System.Collections.ArrayList) this.clauses.Clone();
			return clone;
		}
		
		/// <summary>Prints a user-readable version of this query. </summary>
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			if (GetBoost() != 1.0)
			{
				buffer.Append("(");
			}
			
			for (int i = 0; i < clauses.Count; i++)
			{
				BooleanClause c = (BooleanClause) clauses[i];
				if (c.prohibited)
					buffer.Append("-");
				else if (c.required)
					buffer.Append("+");
				
				Query subQuery = c.query;
				if (subQuery is BooleanQuery)
				{
					// wrap sub-bools in parens
					buffer.Append("(");
					buffer.Append(c.query.ToString(field));
					buffer.Append(")");
				}
				else
					buffer.Append(c.query.ToString(field));
				
				if (i != clauses.Count - 1)
					buffer.Append(" ");
			}
			
			if (GetBoost() != 1.0)
			{
                System.Globalization.NumberFormatInfo nfi = new System.Globalization.CultureInfo("en-US", false).NumberFormat;
                nfi.NumberDecimalDigits = 1;

                buffer.Append(")^");
                buffer.Append(GetBoost().ToString("N", nfi));

                //buffer.Append(")^");
				//buffer.Append(GetBoost());
			}
			
			return buffer.ToString();
		}
		
		/// <summary>Returns true iff <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (!(o is BooleanQuery))
				return false;
			BooleanQuery other = (BooleanQuery) o;
			return (this.GetBoost() == other.GetBoost()) && this.clauses.Equals(other.clauses);
		}
		
		/// <summary>Returns a hash code value for this object.</summary>
		public override int GetHashCode()
		{
            int boostInt = BitConverter.ToInt32(BitConverter.GetBytes(GetBoost()), 0);
            return boostInt ^ clauses.GetHashCode();
		}
	}
}