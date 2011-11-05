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
using Query = Mono.Lucene.Net.Search.Query;

namespace Mono.Lucene.Net.Search.Spans
{
	
	/// <summary>Matches spans which are near one another.  One can specify <i>slop</i>, the
	/// maximum number of intervening unmatched positions, as well as whether
	/// matches are required to be in-order. 
	/// </summary>
	[Serializable]
	public class SpanNearQuery:SpanQuery, System.ICloneable
	{
		protected internal System.Collections.ArrayList clauses;
		protected internal int slop;
		protected internal bool inOrder;
		
		protected internal System.String field;
		private bool collectPayloads;
		
		/// <summary>Construct a SpanNearQuery.  Matches spans matching a span from each
		/// clause, with up to <code>slop</code> total unmatched positions between
		/// them.  * When <code>inOrder</code> is true, the spans from each clause
		/// must be * ordered as in <code>clauses</code>. 
		/// </summary>
		public SpanNearQuery(SpanQuery[] clauses, int slop, bool inOrder):this(clauses, slop, inOrder, true)
		{
		}
		
		public SpanNearQuery(SpanQuery[] clauses, int slop, bool inOrder, bool collectPayloads)
		{
			
			// copy clauses array into an ArrayList
			this.clauses = new System.Collections.ArrayList(clauses.Length);
			for (int i = 0; i < clauses.Length; i++)
			{
				SpanQuery clause = clauses[i];
				if (i == 0)
				{
					// check field
					field = clause.GetField();
				}
				else if (!clause.GetField().Equals(field))
				{
					throw new System.ArgumentException("Clauses must have same field.");
				}
				this.clauses.Add(clause);
			}
			this.collectPayloads = collectPayloads;
			this.slop = slop;
			this.inOrder = inOrder;
		}
		
		/// <summary>Return the clauses whose spans are matched. </summary>
		public virtual SpanQuery[] GetClauses()
		{
			return (SpanQuery[]) clauses.ToArray(typeof(SpanQuery));
		}
		
		/// <summary>Return the maximum number of intervening unmatched positions permitted.</summary>
		public virtual int GetSlop()
		{
			return slop;
		}
		
		/// <summary>Return true if matches are required to be in-order.</summary>
		public virtual bool IsInOrder()
		{
			return inOrder;
		}
		
		public override System.String GetField()
		{
			return field;
		}
		
		/// <summary>Returns a collection of all terms matched by this query.</summary>
		/// <deprecated> use extractTerms instead
		/// </deprecated>
		/// <seealso cref="ExtractTerms(Set)">
		/// </seealso>
        [Obsolete("use ExtractTerms instead")]
		public override System.Collections.ICollection GetTerms()
		{
			System.Collections.ArrayList terms = new System.Collections.ArrayList();
			System.Collections.IEnumerator i = clauses.GetEnumerator();
			while (i.MoveNext())
			{
				SpanQuery clause = (SpanQuery) i.Current;
				terms.AddRange(clause.GetTerms());
			}
			return terms;
		}
		
		public override void  ExtractTerms(System.Collections.Hashtable terms)
		{
            foreach (SpanQuery clause in clauses)
            {
                clause.ExtractTerms(terms);
            }
		}
		
		
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("spanNear([");
			System.Collections.IEnumerator i = clauses.GetEnumerator();
			while (i.MoveNext())
			{
				SpanQuery clause = (SpanQuery) i.Current;
				buffer.Append(clause.ToString(field));
                buffer.Append(", ");
			}
            if (clauses.Count > 0) buffer.Length -= 2;
			buffer.Append("], ");
			buffer.Append(slop);
			buffer.Append(", ");
			buffer.Append(inOrder);
			buffer.Append(")");
			buffer.Append(ToStringUtils.Boost(GetBoost()));
			return buffer.ToString();
		}
		
		public override Spans GetSpans(IndexReader reader)
		{
			if (clauses.Count == 0)
			// optimize 0-clause case
				return new SpanOrQuery(GetClauses()).GetSpans(reader);
			
			if (clauses.Count == 1)
			// optimize 1-clause case
				return ((SpanQuery) clauses[0]).GetSpans(reader);
			
			return inOrder?(Spans) new NearSpansOrdered(this, reader, collectPayloads):(Spans) new NearSpansUnordered(this, reader);
		}
		
		public override Query Rewrite(IndexReader reader)
		{
			SpanNearQuery clone = null;
			for (int i = 0; i < clauses.Count; i++)
			{
				SpanQuery c = (SpanQuery) clauses[i];
				SpanQuery query = (SpanQuery) c.Rewrite(reader);
				if (query != c)
				{
					// clause rewrote: must clone
					if (clone == null)
						clone = (SpanNearQuery) this.Clone();
					clone.clauses[i] = query;
				}
			}
			if (clone != null)
			{
				return clone; // some clauses rewrote
			}
			else
			{
				return this; // no clauses rewrote
			}
		}
		
		public override System.Object Clone()
		{
			int sz = clauses.Count;
			SpanQuery[] newClauses = new SpanQuery[sz];
			
			for (int i = 0; i < sz; i++)
			{
				SpanQuery clause = (SpanQuery) clauses[i];
				newClauses[i] = (SpanQuery) clause.Clone();
			}
			SpanNearQuery spanNearQuery = new SpanNearQuery(newClauses, slop, inOrder);
			spanNearQuery.SetBoost(GetBoost());
			return spanNearQuery;
		}
		
		/// <summary>Returns true iff <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (this == o)
				return true;
			if (!(o is SpanNearQuery))
				return false;
			
			SpanNearQuery spanNearQuery = (SpanNearQuery) o;
			
			if (inOrder != spanNearQuery.inOrder)
				return false;
			if (slop != spanNearQuery.slop)
				return false;
			if (clauses.Count != spanNearQuery.clauses.Count)
				return false;
            System.Collections.IEnumerator iter1 = clauses.GetEnumerator();
            System.Collections.IEnumerator iter2 = spanNearQuery.clauses.GetEnumerator();
            while (iter1.MoveNext() && iter2.MoveNext())
            {
                SpanQuery item1 = (SpanQuery)iter1.Current;
                SpanQuery item2 = (SpanQuery)iter2.Current;
                if (!item1.Equals(item2))
                    return false;
            }
			
			return GetBoost() == spanNearQuery.GetBoost();
		}
		
		public override int GetHashCode()
		{
			long result = 0;
            //mgarski .NET uses the arraylist's location, not contents to calculate the hash
            // need to start with result being the hash of the contents.
            foreach (SpanQuery sq in clauses)
            {
                result += sq.GetHashCode();
            }
			// Mix bits before folding in things like boost, since it could cancel the
			// last element of clauses.  This particular mix also serves to
			// differentiate SpanNearQuery hashcodes from others.
			result ^= ((result << 14) | (SupportClass.Number.URShift(result, 19))); // reversible
			result += System.Convert.ToInt32(GetBoost());
			result += slop;
			result ^= (inOrder ? (long) 0x99AFD3BD : 0);
			return (int) result;
		}
	}
}
