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

using System.Runtime.InteropServices;
using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using Term = Mono.Lucene.Net.Index.Term;
using QueryParser = Mono.Lucene.Net.QueryParsers.QueryParser;
using ToStringUtils = Mono.Lucene.Net.Util.ToStringUtils;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> An abstract {@link Query} that matches documents
	/// containing a subset of terms provided by a {@link
	/// FilteredTermEnum} enumeration.
	/// 
	/// <p/>This query cannot be used directly; you must subclass
	/// it and define {@link #getEnum} to provide a {@link
	/// FilteredTermEnum} that iterates through the terms to be
	/// matched.
	/// 
	/// <p/><b>NOTE</b>: if {@link #setRewriteMethod} is either
	/// {@link #CONSTANT_SCORE_BOOLEAN_QUERY_REWRITE} or {@link
	/// #SCORING_BOOLEAN_QUERY_REWRITE}, you may encounter a
	/// {@link BooleanQuery.TooManyClauses} exception during
	/// searching, which happens when the number of terms to be
	/// searched exceeds {@link
	/// BooleanQuery#GetMaxClauseCount()}.  Setting {@link
	/// #setRewriteMethod} to {@link #CONSTANT_SCORE_FILTER_REWRITE}
	/// prevents this.
	/// 
	/// <p/>The recommended rewrite method is {@link
	/// #CONSTANT_SCORE_AUTO_REWRITE_DEFAULT}: it doesn't spend CPU
	/// computing unhelpful scores, and it tries to pick the most
	/// performant rewrite method given the query.
	/// 
	/// Note that {@link QueryParser} produces
	/// MultiTermQueries using {@link
	/// #CONSTANT_SCORE_AUTO_REWRITE_DEFAULT} by default.
	/// </summary>
	[Serializable]
	public abstract class MultiTermQuery:Query
	{
		[Serializable]
		public class AnonymousClassConstantScoreAutoRewrite:ConstantScoreAutoRewrite
		{
			public override void  SetTermCountCutoff(int count)
			{
				throw new System.NotSupportedException("Please create a private instance");
			}
			
			public override void  SetDocCountPercent(double percent)
			{
				throw new System.NotSupportedException("Please create a private instance");
			}
			
			// Make sure we are still a singleton even after deserializing
			protected internal virtual System.Object ReadResolve()
			{
				return Mono.Lucene.Net.Search.MultiTermQuery.CONSTANT_SCORE_AUTO_REWRITE_DEFAULT;
			}
		}
		/* @deprecated move to sub class */
		protected internal Term term;
		protected internal RewriteMethod rewriteMethod = CONSTANT_SCORE_AUTO_REWRITE_DEFAULT;
		[NonSerialized]
		internal int numberOfTerms = 0;
		
		/// <summary>Abstract class that defines how the query is rewritten. </summary>
		[Serializable]
		public abstract class RewriteMethod
		{
			public abstract Query Rewrite(IndexReader reader, MultiTermQuery query);
		}
		
		[Serializable]
		private sealed class ConstantScoreFilterRewrite:RewriteMethod
		{
			public override Query Rewrite(IndexReader reader, MultiTermQuery query)
			{
				Query result = new ConstantScoreQuery(new MultiTermQueryWrapperFilter(query));
				result.SetBoost(query.GetBoost());
				return result;
			}
			
			// Make sure we are still a singleton even after deserializing
			internal System.Object ReadResolve()
			{
				return Mono.Lucene.Net.Search.MultiTermQuery.CONSTANT_SCORE_FILTER_REWRITE;
			}
		}
		
		/// <summary>A rewrite method that first creates a private Filter,
		/// by visiting each term in sequence and marking all docs
		/// for that term.  Matching documents are assigned a
		/// constant score equal to the query's boost.
		/// 
		/// <p/> This method is faster than the BooleanQuery
		/// rewrite methods when the number of matched terms or
		/// matched documents is non-trivial. Also, it will never
		/// hit an errant {@link BooleanQuery.TooManyClauses}
		/// exception.
		/// 
		/// </summary>
		/// <seealso cref="setRewriteMethod">
		/// </seealso>
		public static readonly RewriteMethod CONSTANT_SCORE_FILTER_REWRITE = new ConstantScoreFilterRewrite();
		
		[Serializable]
		private class ScoringBooleanQueryRewrite:RewriteMethod
		{
			public override Query Rewrite(IndexReader reader, MultiTermQuery query)
			{
				
				FilteredTermEnum enumerator = query.GetEnum(reader);
				BooleanQuery result = new BooleanQuery(true);
				int count = 0;
				try
				{
					do 
					{
						Term t = enumerator.Term();
						if (t != null)
						{
							TermQuery tq = new TermQuery(t); // found a match
							tq.SetBoost(query.GetBoost() * enumerator.Difference()); // set the boost
							result.Add(tq, BooleanClause.Occur.SHOULD); // add to query
							count++;
						}
					}
					while (enumerator.Next());
				}
				finally
				{
					enumerator.Close();
				}
				query.IncTotalNumberOfTerms(count);
				return result;
			}
			
			// Make sure we are still a singleton even after deserializing
			protected internal virtual System.Object ReadResolve()
			{
				return Mono.Lucene.Net.Search.MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE;
			}
		}
		
		/// <summary>A rewrite method that first translates each term into
		/// {@link BooleanClause.Occur#SHOULD} clause in a
		/// BooleanQuery, and keeps the scores as computed by the
		/// query.  Note that typically such scores are
		/// meaningless to the user, and require non-trivial CPU
		/// to compute, so it's almost always better to use {@link
		/// #CONSTANT_SCORE_AUTO_REWRITE_DEFAULT} instead.
		/// 
		/// <p/><b>NOTE</b>: This rewrite method will hit {@link
		/// BooleanQuery.TooManyClauses} if the number of terms
		/// exceeds {@link BooleanQuery#getMaxClauseCount}.
		/// 
		/// </summary>
		/// <seealso cref="setRewriteMethod">
		/// </seealso>
		public static readonly RewriteMethod SCORING_BOOLEAN_QUERY_REWRITE = new ScoringBooleanQueryRewrite();
		
		[Serializable]
		private class ConstantScoreBooleanQueryRewrite:ScoringBooleanQueryRewrite
		{
			public override Query Rewrite(IndexReader reader, MultiTermQuery query)
			{
				// strip the scores off
				Query result = new ConstantScoreQuery(new QueryWrapperFilter(base.Rewrite(reader, query)));
				result.SetBoost(query.GetBoost());
				return result;
			}
			
			// Make sure we are still a singleton even after deserializing
			protected internal override System.Object ReadResolve()
			{
				return Mono.Lucene.Net.Search.MultiTermQuery.CONSTANT_SCORE_BOOLEAN_QUERY_REWRITE;
			}
		}
		
		/// <summary>Like {@link #SCORING_BOOLEAN_QUERY_REWRITE} except
		/// scores are not computed.  Instead, each matching
		/// document receives a constant score equal to the
		/// query's boost.
		/// 
		/// <p/><b>NOTE</b>: This rewrite method will hit {@link
		/// BooleanQuery.TooManyClauses} if the number of terms
		/// exceeds {@link BooleanQuery#getMaxClauseCount}.
		/// 
		/// </summary>
		/// <seealso cref="setRewriteMethod">
		/// </seealso>
		public static readonly RewriteMethod CONSTANT_SCORE_BOOLEAN_QUERY_REWRITE = new ConstantScoreBooleanQueryRewrite();
		
		
		/// <summary>A rewrite method that tries to pick the best
		/// constant-score rewrite method based on term and
		/// document counts from the query.  If both the number of
		/// terms and documents is small enough, then {@link
		/// #CONSTANT_SCORE_BOOLEAN_QUERY_REWRITE} is used.
		/// Otherwise, {@link #CONSTANT_SCORE_FILTER_REWRITE} is
		/// used.
		/// </summary>
		[Serializable]
		public class ConstantScoreAutoRewrite:RewriteMethod
		{
			public ConstantScoreAutoRewrite()
			{
				InitBlock();
			}
			private void  InitBlock()
			{
				termCountCutoff = DEFAULT_TERM_COUNT_CUTOFF;
				docCountPercent = DEFAULT_DOC_COUNT_PERCENT;
			}
			
			// Defaults derived from rough tests with a 20.0 million
			// doc Wikipedia index.  With more than 350 terms in the
			// query, the filter method is fastest:
			public static int DEFAULT_TERM_COUNT_CUTOFF = 350;
			
			// If the query will hit more than 1 in 1000 of the docs
			// in the index (0.1%), the filter method is fastest:
			public static double DEFAULT_DOC_COUNT_PERCENT = 0.1;
			
			private int termCountCutoff;
			private double docCountPercent;
			
			/// <summary>If the number of terms in this query is equal to or
			/// larger than this setting then {@link
			/// #CONSTANT_SCORE_FILTER_REWRITE} is used. 
			/// </summary>
			public virtual void  SetTermCountCutoff(int count)
			{
				termCountCutoff = count;
			}
			
			/// <seealso cref="setTermCountCutoff">
			/// </seealso>
			public virtual int GetTermCountCutoff()
			{
				return termCountCutoff;
			}
			
			/// <summary>If the number of documents to be visited in the
			/// postings exceeds this specified percentage of the
			/// maxDoc() for the index, then {@link
			/// #CONSTANT_SCORE_FILTER_REWRITE} is used.
			/// </summary>
			/// <param name="percent">0.0 to 100.0 
			/// </param>
			public virtual void  SetDocCountPercent(double percent)
			{
				docCountPercent = percent;
			}
			
			/// <seealso cref="setDocCountPercent">
			/// </seealso>
			public virtual double GetDocCountPercent()
			{
				return docCountPercent;
			}
			
			public override Query Rewrite(IndexReader reader, MultiTermQuery query)
			{
				// Get the enum and start visiting terms.  If we
				// exhaust the enum before hitting either of the
				// cutoffs, we use ConstantBooleanQueryRewrite; else,
				// ConstantFilterRewrite:
				System.Collections.ArrayList pendingTerms = new System.Collections.ArrayList();
				int docCountCutoff = (int) ((docCountPercent / 100.0) * reader.MaxDoc());
				int termCountLimit = System.Math.Min(BooleanQuery.GetMaxClauseCount(), termCountCutoff);
				int docVisitCount = 0;
				
				FilteredTermEnum enumerator = query.GetEnum(reader);
				try
				{
					while (true)
					{
						Term t = enumerator.Term();
						if (t != null)
						{
							pendingTerms.Add(t);
							// Loading the TermInfo from the terms dict here
							// should not be costly, because 1) the
							// query/filter will load the TermInfo when it
							// runs, and 2) the terms dict has a cache:
							docVisitCount += reader.DocFreq(t);
						}
						
						if (pendingTerms.Count >= termCountLimit || docVisitCount >= docCountCutoff)
						{
							// Too many terms -- make a filter.
							Query result = new ConstantScoreQuery(new MultiTermQueryWrapperFilter(query));
							result.SetBoost(query.GetBoost());
							return result;
						}
						else if (!enumerator.Next())
						{
							// Enumeration is done, and we hit a small
							// enough number of terms & docs -- just make a
							// BooleanQuery, now
							System.Collections.IEnumerator it = pendingTerms.GetEnumerator();
							BooleanQuery bq = new BooleanQuery(true);
							while (it.MoveNext())
							{
								TermQuery tq = new TermQuery((Term) it.Current);
								bq.Add(tq, BooleanClause.Occur.SHOULD);
							}
							// Strip scores
							Query result = new ConstantScoreQuery(new QueryWrapperFilter(bq));
							result.SetBoost(query.GetBoost());
							query.IncTotalNumberOfTerms(pendingTerms.Count);
							return result;
						}
					}
				}
				finally
				{
					enumerator.Close();
				}
			}
			
			public override int GetHashCode()
			{
				int prime = 1279;
				return (int) (prime * termCountCutoff + BitConverter.DoubleToInt64Bits(docCountPercent));
			}
			
			public  override bool Equals(System.Object obj)
			{
				if (this == obj)
					return true;
				if (obj == null)
					return false;
				if (GetType() != obj.GetType())
					return false;
				
				ConstantScoreAutoRewrite other = (ConstantScoreAutoRewrite) obj;
				if (other.termCountCutoff != termCountCutoff)
				{
					return false;
				}
				
				if (BitConverter.DoubleToInt64Bits(other.docCountPercent) != BitConverter.DoubleToInt64Bits(docCountPercent))
				{
					return false;
				}
				
				return true;
			}
		}
		
		/// <summary>Read-only default instance of {@link
		/// ConstantScoreAutoRewrite}, with {@link
		/// ConstantScoreAutoRewrite#setTermCountCutoff} set to
		/// {@link
		/// ConstantScoreAutoRewrite#DEFAULT_TERM_COUNT_CUTOFF}
		/// and {@link
		/// ConstantScoreAutoRewrite#setDocCountPercent} set to
		/// {@link
		/// ConstantScoreAutoRewrite#DEFAULT_DOC_COUNT_PERCENT}.
		/// Note that you cannot alter the configuration of this
		/// instance; you'll need to create a private instance
		/// instead. 
		/// </summary>
		public static readonly RewriteMethod CONSTANT_SCORE_AUTO_REWRITE_DEFAULT;
		
		/// <summary> Constructs a query for terms matching <code>term</code>.</summary>
		/// <deprecated> check sub class for possible term access - the Term does not
		/// make sense for all MultiTermQuerys and will be removed.
		/// </deprecated>
        [Obsolete("check sub class for possible term access - the Term does not make sense for all MultiTermQuerys and will be removed.")]
		public MultiTermQuery(Term term)
		{
			this.term = term;
		}
		
		/// <summary> Constructs a query matching terms that cannot be represented with a single
		/// Term.
		/// </summary>
		public MultiTermQuery()
		{
		}
		
		/// <summary> Returns the pattern term.</summary>
		/// <deprecated> check sub class for possible term access - getTerm does not
		/// make sense for all MultiTermQuerys and will be removed.
		/// </deprecated>
        [Obsolete("check sub class for possible term access - getTerm does not make sense for all MultiTermQuerys and will be removed.")]
		public virtual Term GetTerm()
		{
			return term;
		}
		
		/// <summary>Construct the enumeration to be used, expanding the pattern term. </summary>
		public /*protected internal*/ abstract FilteredTermEnum GetEnum(IndexReader reader);
		
		/// <summary> Expert: Return the number of unique terms visited during execution of the query.
		/// If there are many of them, you may consider using another query type
		/// or optimize your total term count in index.
		/// <p/>This method is not thread safe, be sure to only call it when no query is running!
		/// If you re-use the same query instance for another
		/// search, be sure to first reset the term counter
		/// with {@link #clearTotalNumberOfTerms}.
		/// <p/>On optimized indexes / no MultiReaders, you get the correct number of
		/// unique terms for the whole index. Use this number to compare different queries.
		/// For non-optimized indexes this number can also be achived in
		/// non-constant-score mode. In constant-score mode you get the total number of
		/// terms seeked for all segments / sub-readers.
		/// </summary>
		/// <seealso cref="clearTotalNumberOfTerms">
		/// </seealso>
		public virtual int GetTotalNumberOfTerms()
		{
			return numberOfTerms;
		}
		
		/// <summary> Expert: Resets the counting of unique terms.
		/// Do this before executing the query/filter.
		/// </summary>
		/// <seealso cref="getTotalNumberOfTerms">
		/// </seealso>
		public virtual void  ClearTotalNumberOfTerms()
		{
			numberOfTerms = 0;
		}
		
		protected internal virtual void  IncTotalNumberOfTerms(int inc)
		{
			numberOfTerms += inc;
		}
		
		public override Query Rewrite(IndexReader reader)
		{
			return rewriteMethod.Rewrite(reader, this);
		}
		
		
		/* Prints a user-readable version of this query.
		* Implemented for back compat in case MultiTermQuery
		* subclasses do no implement.
		*/
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			if (term != null)
			{
				if (!term.Field().Equals(field))
				{
					buffer.Append(term.Field());
					buffer.Append(":");
				}
				buffer.Append(term.Text());
			}
			else
			{
				buffer.Append("termPattern:unknown");
			}
			buffer.Append(ToStringUtils.Boost(GetBoost()));
			return buffer.ToString();
		}
		
		/// <seealso cref="setRewriteMethod">
		/// </seealso>
		public virtual RewriteMethod GetRewriteMethod()
		{
			return rewriteMethod;
		}
		
		/// <summary> Sets the rewrite method to be used when executing the
		/// query.  You can use one of the four core methods, or
		/// implement your own subclass of {@link RewriteMethod}. 
		/// </summary>
		public virtual void  SetRewriteMethod(RewriteMethod method)
		{
			rewriteMethod = method;
		}
		
		//@Override
		public override int GetHashCode()
		{
			int prime = 31;
			int result = 1;
			result = prime * result + System.Convert.ToInt32(GetBoost());
			result = prime * result;
			result += rewriteMethod.GetHashCode();
			return result;
		}
		
		//@Override
		public  override bool Equals(System.Object obj)
		{
			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (GetType() != obj.GetType())
				return false;
			MultiTermQuery other = (MultiTermQuery) obj;
			if (System.Convert.ToInt32(GetBoost()) != System.Convert.ToInt32(other.GetBoost()))
				return false;
			if (!rewriteMethod.Equals(other.rewriteMethod))
			{
				return false;
			}
			return true;
		}
		static MultiTermQuery()
		{
			CONSTANT_SCORE_AUTO_REWRITE_DEFAULT = new AnonymousClassConstantScoreAutoRewrite();
		}
	}
}
