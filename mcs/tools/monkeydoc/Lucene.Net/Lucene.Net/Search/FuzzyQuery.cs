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
using Term = Mono.Lucene.Net.Index.Term;
using PriorityQueue = Mono.Lucene.Net.Util.PriorityQueue;
using ToStringUtils = Mono.Lucene.Net.Util.ToStringUtils;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary>Implements the fuzzy search query. The similarity measurement
	/// is based on the Levenshtein (edit distance) algorithm.
	/// 
	/// Warning: this query is not very scalable with its default prefix
	/// length of 0 - in this case, *every* term will be enumerated and
	/// cause an edit score calculation.
	/// 
	/// </summary>
	[Serializable]
	public class FuzzyQuery:MultiTermQuery
	{
		
		public const float defaultMinSimilarity = 0.5f;
		public const int defaultPrefixLength = 0;
		
		private float minimumSimilarity;
		private int prefixLength;
		private bool termLongEnough = false;
		
		new protected internal Term term;
		
		/// <summary> Create a new FuzzyQuery that will match terms with a similarity 
		/// of at least <code>minimumSimilarity</code> to <code>term</code>.
		/// If a <code>prefixLength</code> &gt; 0 is specified, a common prefix
		/// of that length is also required.
		/// 
		/// </summary>
		/// <param name="term">the term to search for
		/// </param>
		/// <param name="minimumSimilarity">a value between 0 and 1 to set the required similarity
		/// between the query term and the matching terms. For example, for a
		/// <code>minimumSimilarity</code> of <code>0.5</code> a term of the same length
		/// as the query term is considered similar to the query term if the edit distance
		/// between both terms is less than <code>length(term)*0.5</code>
		/// </param>
		/// <param name="prefixLength">length of common (non-fuzzy) prefix
		/// </param>
		/// <throws>  IllegalArgumentException if minimumSimilarity is &gt;= 1 or &lt; 0 </throws>
		/// <summary> or if prefixLength &lt; 0
		/// </summary>
		public FuzzyQuery(Term term, float minimumSimilarity, int prefixLength):base(term)
		{ // will be removed in 3.0
			this.term = term;
			
			if (minimumSimilarity >= 1.0f)
				throw new System.ArgumentException("minimumSimilarity >= 1");
			else if (minimumSimilarity < 0.0f)
				throw new System.ArgumentException("minimumSimilarity < 0");
			if (prefixLength < 0)
				throw new System.ArgumentException("prefixLength < 0");
			
			if (term.Text().Length > 1.0f / (1.0f - minimumSimilarity))
			{
				this.termLongEnough = true;
			}
			
			this.minimumSimilarity = minimumSimilarity;
			this.prefixLength = prefixLength;
			rewriteMethod = SCORING_BOOLEAN_QUERY_REWRITE;
		}
		
		/// <summary> Calls {@link #FuzzyQuery(Term, float) FuzzyQuery(term, minimumSimilarity, 0)}.</summary>
		public FuzzyQuery(Term term, float minimumSimilarity):this(term, minimumSimilarity, defaultPrefixLength)
		{
		}
		
		/// <summary> Calls {@link #FuzzyQuery(Term, float) FuzzyQuery(term, 0.5f, 0)}.</summary>
		public FuzzyQuery(Term term):this(term, defaultMinSimilarity, defaultPrefixLength)
		{
		}
		
		/// <summary> Returns the minimum similarity that is required for this query to match.</summary>
		/// <returns> float value between 0.0 and 1.0
		/// </returns>
		public virtual float GetMinSimilarity()
		{
			return minimumSimilarity;
		}
		
		/// <summary> Returns the non-fuzzy prefix length. This is the number of characters at the start
		/// of a term that must be identical (not fuzzy) to the query term if the query
		/// is to match that term. 
		/// </summary>
		public virtual int GetPrefixLength()
		{
			return prefixLength;
		}
		
		public /*protected internal*/ override FilteredTermEnum GetEnum(IndexReader reader)
		{
			return new FuzzyTermEnum(reader, GetTerm(), minimumSimilarity, prefixLength);
		}
		
		/// <summary> Returns the pattern term.</summary>
        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Search.MultiTermQuery.GetTerm()")]
		public override Term GetTerm()
		{
			return term;
		}
		
		public override void  SetRewriteMethod(RewriteMethod method)
		{
			throw new System.NotSupportedException("FuzzyQuery cannot change rewrite method");
		}
		
		public override Query Rewrite(IndexReader reader)
		{
			if (!termLongEnough)
			{
				// can only match if it's exact
				return new TermQuery(term);
			}
			
			FilteredTermEnum enumerator = GetEnum(reader);
			int maxClauseCount = BooleanQuery.GetMaxClauseCount();
			ScoreTermQueue stQueue = new ScoreTermQueue(maxClauseCount);
			ScoreTerm reusableST = null;
			
			try
			{
				do 
				{
					float score = 0.0f;
					Term t = enumerator.Term();
					if (t != null)
					{
						score = enumerator.Difference();
						if (reusableST == null)
						{
							reusableST = new ScoreTerm(t, score);
						}
						else if (score >= reusableST.score)
						{
							// reusableST holds the last "rejected" entry, so, if
							// this new score is not better than that, there's no
							// need to try inserting it
							reusableST.score = score;
							reusableST.term = t;
						}
						else
						{
							continue;
						}
						
						reusableST = (ScoreTerm) stQueue.InsertWithOverflow(reusableST);
					}
				}
				while (enumerator.Next());
			}
			finally
			{
				enumerator.Close();
			}
			
			BooleanQuery query = new BooleanQuery(true);
			int size = stQueue.Size();
			for (int i = 0; i < size; i++)
			{
				ScoreTerm st = (ScoreTerm) stQueue.Pop();
				TermQuery tq = new TermQuery(st.term); // found a match
				tq.SetBoost(GetBoost() * st.score); // set the boost
				query.Add(tq, BooleanClause.Occur.SHOULD); // add to query
			}
			
			return query;
		}
		
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			if (!term.Field().Equals(field))
			{
				buffer.Append(term.Field());
				buffer.Append(":");
			}
			buffer.Append(term.Text());
			buffer.Append('~');
			buffer.Append(SupportClass.Single.ToString(minimumSimilarity));
			buffer.Append(ToStringUtils.Boost(GetBoost()));
			return buffer.ToString();
		}
		
		protected internal class ScoreTerm
		{
			public Term term;
			public float score;
			
			public ScoreTerm(Term term, float score)
			{
				this.term = term;
				this.score = score;
			}
		}
		
		protected internal class ScoreTermQueue:PriorityQueue
		{
			
			public ScoreTermQueue(int size)
			{
				Initialize(size);
			}
			
			/* (non-Javadoc)
			* @see Mono.Lucene.Net.Util.PriorityQueue#lessThan(java.lang.Object, java.lang.Object)
			*/
			public override bool LessThan(System.Object a, System.Object b)
			{
				ScoreTerm termA = (ScoreTerm) a;
				ScoreTerm termB = (ScoreTerm) b;
				if (termA.score == termB.score)
					return termA.term.CompareTo(termB.term) > 0;
				else
					return termA.score < termB.score;
			}
		}
		
		public override int GetHashCode()
		{
			int prime = 31;
			int result = base.GetHashCode();
			result = prime * result + BitConverter.ToInt32(BitConverter.GetBytes(minimumSimilarity), 0);
			result = prime * result + prefixLength;
			result = prime * result + ((term == null)?0:term.GetHashCode());
			return result;
		}
		
		public  override bool Equals(System.Object obj)
		{
			if (this == obj)
				return true;
			if (!base.Equals(obj))
				return false;
			if (GetType() != obj.GetType())
				return false;
			FuzzyQuery other = (FuzzyQuery) obj;
			if (BitConverter.ToInt32(BitConverter.GetBytes(minimumSimilarity), 0) != BitConverter.ToInt32(BitConverter.GetBytes(other.minimumSimilarity), 0))
				return false;
			if (prefixLength != other.prefixLength)
				return false;
			if (term == null)
			{
				if (other.term != null)
					return false;
			}
			else if (!term.Equals(other.term))
				return false;
			return true;
		}
	}
}
