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
using ComplexExplanation = Mono.Lucene.Net.Search.ComplexExplanation;
using Explanation = Mono.Lucene.Net.Search.Explanation;
using Query = Mono.Lucene.Net.Search.Query;
using Scorer = Mono.Lucene.Net.Search.Scorer;
using Searcher = Mono.Lucene.Net.Search.Searcher;
using Similarity = Mono.Lucene.Net.Search.Similarity;
using Weight = Mono.Lucene.Net.Search.Weight;

namespace Mono.Lucene.Net.Search.Function
{
	
	/// <summary> Query that sets document score as a programmatic function of several (sub) scores:
	/// <ol>
	/// <li>the score of its subQuery (any query)</li>
	/// <li>(optional) the score of its ValueSourceQuery (or queries).
	/// For most simple/convenient use cases this query is likely to be a 
	/// {@link Mono.Lucene.Net.Search.Function.FieldScoreQuery FieldScoreQuery}</li>
	/// </ol>
    /// Subclasses can modify the computation by overriding {@link #getCustomScoreProvider}.
	/// 
	/// <p/><font color="#FF0000">
	/// WARNING: The status of the <b>Search.Function</b> package is experimental. 
	/// The APIs introduced here might change in the future and will not be 
	/// supported anymore in such a case.</font>
	/// </summary>
	[Serializable]
	public class CustomScoreQuery:Query, System.ICloneable
	{
		
		private Query subQuery;
		private ValueSourceQuery[] valSrcQueries; // never null (empty array if there are no valSrcQueries).
		private bool strict = false; // if true, valueSource part of query does not take part in weights normalization.  
		
		/// <summary> Create a CustomScoreQuery over input subQuery.</summary>
		/// <param name="subQuery">the sub query whose scored is being customed. Must not be null. 
		/// </param>
		public CustomScoreQuery(Query subQuery):this(subQuery, new ValueSourceQuery[0])
		{
		}
		
		/// <summary> Create a CustomScoreQuery over input subQuery and a {@link ValueSourceQuery}.</summary>
		/// <param name="subQuery">the sub query whose score is being customed. Must not be null.
		/// </param>
		/// <param name="valSrcQuery">a value source query whose scores are used in the custom score
		/// computation. For most simple/convineient use case this would be a 
		/// {@link Mono.Lucene.Net.Search.Function.FieldScoreQuery FieldScoreQuery}.
        /// This parameter is optional - it can be null or even an empty array.
		/// </param>
		public CustomScoreQuery(Query subQuery, ValueSourceQuery valSrcQuery):this(subQuery, valSrcQuery != null?new ValueSourceQuery[]{valSrcQuery}:new ValueSourceQuery[0])
		{
		}
		
		/// <summary> Create a CustomScoreQuery over input subQuery and a {@link ValueSourceQuery}.</summary>
		/// <param name="subQuery">the sub query whose score is being customized. Must not be null.
		/// </param>
		/// <param name="valSrcQueries">value source queries whose scores are used in the custom score
		/// computation. For most simple/convenient use case these would be 
		/// {@link Mono.Lucene.Net.Search.Function.FieldScoreQuery FieldScoreQueries}.
		/// This parameter is optional - it can be null or even an empty array.
		/// </param>
		public CustomScoreQuery(Query subQuery, ValueSourceQuery[] valSrcQueries)
		{
			this.subQuery = subQuery;
			this.valSrcQueries = valSrcQueries != null?valSrcQueries:new ValueSourceQuery[0];
			if (subQuery == null)
				throw new System.ArgumentException("<subquery> must not be null!");
		}
		
		/*(non-Javadoc) @see Mono.Lucene.Net.Search.Query#rewrite(Mono.Lucene.Net.Index.IndexReader) */
        public override Query Rewrite(IndexReader reader)
        {
            CustomScoreQuery clone = null;

            Query sq = subQuery.Rewrite(reader);
            if (sq != subQuery)
            {
                clone = (CustomScoreQuery)Clone();
                clone.subQuery = sq;
            }

            for (int i = 0; i < valSrcQueries.Length; i++)
            {
                ValueSourceQuery v = (ValueSourceQuery)valSrcQueries[i].Rewrite(reader);
                if (v != valSrcQueries[i])
                {
                    if (clone == null) clone = (CustomScoreQuery)Clone();
                    clone.valSrcQueries[i] = v;
                }
            }

            return (clone == null) ? this : clone;
        }
		
		/*(non-Javadoc) @see Mono.Lucene.Net.Search.Query#extractTerms(java.util.Set) */
		public override void  ExtractTerms(System.Collections.Hashtable terms)
		{
			subQuery.ExtractTerms(terms);
			for (int i = 0; i < valSrcQueries.Length; i++)
			{
				valSrcQueries[i].ExtractTerms(terms);
			}
		}
		
		/*(non-Javadoc) @see Mono.Lucene.Net.Search.Query#clone() */
		public override System.Object Clone()
		{
			CustomScoreQuery clone = (CustomScoreQuery) base.Clone();
			clone.subQuery = (Query) subQuery.Clone();
			clone.valSrcQueries = new ValueSourceQuery[valSrcQueries.Length];
			for (int i = 0; i < valSrcQueries.Length; i++)
			{
				clone.valSrcQueries[i] = (ValueSourceQuery) valSrcQueries[i].Clone();
			}
			return clone;
		}
		
		/* (non-Javadoc) @see Mono.Lucene.Net.Search.Query#toString(java.lang.String) */
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(Name()).Append("(");
			sb.Append(subQuery.ToString(field));
			for (int i = 0; i < valSrcQueries.Length; i++)
			{
				sb.Append(", ").Append(valSrcQueries[i].ToString(field));
			}
			sb.Append(")");
			sb.Append(strict?" STRICT":"");
			return sb.ToString() + ToStringUtils.Boost(GetBoost());
		}
		
		/// <summary>Returns true if <code>o</code> is equal to this. </summary>
		public  override bool Equals(System.Object o)
		{
			if (GetType() != o.GetType())
			{
				return false;
			}
			CustomScoreQuery other = (CustomScoreQuery) o;
            if (this.GetBoost() != other.GetBoost() ||
                !this.subQuery.Equals(other.subQuery) ||
                this.strict != other.strict ||
                this.valSrcQueries.Length != other.valSrcQueries.Length)
            {
                return false;
            }
			for (int i = 0; i < valSrcQueries.Length; i++)
			{
				//TODO simplify with Arrays.deepEquals() once moving to Java 1.5
				if (!valSrcQueries[i].Equals(other.valSrcQueries[i]))
				{
					return false;
				}
			}
			return true;
		}
		
		/// <summary>Returns a hash code value for this object. </summary>
		public override int GetHashCode()
		{
			int valSrcHash = 0;
			for (int i = 0; i < valSrcQueries.Length; i++)
			{
				//TODO simplify with Arrays.deepHashcode() once moving to Java 1.5
				valSrcHash += valSrcQueries[i].GetHashCode();
			}
            return (GetType().GetHashCode() + subQuery.GetHashCode() + valSrcHash) ^
                BitConverter.ToInt32(BitConverter.GetBytes(GetBoost()), 0) ^ (strict ? 1234 : 4321);

		}

        /**
       * Returns a {@link CustomScoreProvider} that calculates the custom scores
       * for the given {@link IndexReader}. The default implementation returns a default
       * implementation as specified in the docs of {@link CustomScoreProvider}.
       * @since 2.9.2
       */
        protected virtual CustomScoreProvider GetCustomScoreProvider(IndexReader reader)
        {
            // when deprecated methods are removed, do not extend class here, just return new default CustomScoreProvider
            return new AnonymousCustomScoreProvider(this, reader);
        }

        class AnonymousCustomScoreProvider : CustomScoreProvider
        {
            CustomScoreQuery parent;
            public AnonymousCustomScoreProvider(CustomScoreQuery parent, IndexReader reader) : base(reader)
            {
                this.parent = parent;
            }
            public override float CustomScore(int doc, float subQueryScore, float[] valSrcScores)
            {
                return parent.CustomScore(doc, subQueryScore, valSrcScores);
            }

            public override float CustomScore(int doc, float subQueryScore, float valSrcScore)
            {
                return parent.CustomScore(doc, subQueryScore, valSrcScore);
            }

            public override Explanation CustomExplain(int doc, Explanation subQueryExpl, Explanation[] valSrcExpls)
            {
                return parent.CustomExplain(doc, subQueryExpl, valSrcExpls);
            }

            public override Explanation CustomExplain(int doc, Explanation subQueryExpl, Explanation valSrcExpl)
            {
                return parent.CustomExplain(doc, subQueryExpl, valSrcExpl);
            }
        }
		
        /// <summary>
        /// Compute a custom score by the subQuery score and a number of 
        /// ValueSourceQuery scores.
        /// 
        /// The doc is relative to the current reader, which is
        /// unknown to CustomScoreQuery when using per-segment search (since Lucene 2.9).
        /// Please override {@link #getCustomScoreProvider} and return a subclass
        /// of {@link CustomScoreProvider} for the given {@link IndexReader}.
        /// see CustomScoreProvider#customScore(int,float,float[])
        /// </summary>
        [Obsolete("Will be removed in Lucene 3.1")]
		public virtual float CustomScore(int doc, float subQueryScore, float[] valSrcScores)
		{
			if (valSrcScores.Length == 1)
			{
				return CustomScore(doc, subQueryScore, valSrcScores[0]);
			}
			if (valSrcScores.Length == 0)
			{
				return CustomScore(doc, subQueryScore, 1);
			}
			float score = subQueryScore;
			for (int i = 0; i < valSrcScores.Length; i++)
			{
				score *= valSrcScores[i];
			}
			return score;
		}
		
		/// <summary> Compute a custom score by the subQuery score and the ValueSourceQuery score.
        /// 
        /// The doc is relative to the current reader, which is
        /// unknown to CustomScoreQuery when using per-segment search (since Lucene 2.9).
        /// Please override {@link #getCustomScoreProvider} and return a subclass
        /// of {@link CustomScoreProvider} for the given {@link IndexReader}.
        /// @see CustomScoreProvider#customScore(int,float,float)
		/// </summary>
        [Obsolete("Will be removed in Lucene 3.1")]
		public virtual float CustomScore(int doc, float subQueryScore, float valSrcScore)
		{
			return subQueryScore * valSrcScore;
		}

        
		
		/// <summary> Explain the custom score.
        /// 
        /// The doc is relative to the current reader, which is
        /// unknown to CustomScoreQuery when using per-segment search (since Lucene 2.9).
        /// Please override {@link #getCustomScoreProvider} and return a subclass
        /// of {@link CustomScoreProvider} for the given {@link IndexReader}.
		/// </summary>
        [Obsolete("Will be removed in Lucene 3.1.")]
		public virtual Explanation CustomExplain(int doc, Explanation subQueryExpl, Explanation[] valSrcExpls)
		{
			if (valSrcExpls.Length == 1)
			{
				return CustomExplain(doc, subQueryExpl, valSrcExpls[0]);
			}
			if (valSrcExpls.Length == 0)
			{
				return subQueryExpl;
			}
			float valSrcScore = 1;
			for (int i = 0; i < valSrcExpls.Length; i++)
			{
				valSrcScore *= valSrcExpls[i].GetValue();
			}
			Explanation exp = new Explanation(valSrcScore * subQueryExpl.GetValue(), "custom score: product of:");
			exp.AddDetail(subQueryExpl);
			for (int i = 0; i < valSrcExpls.Length; i++)
			{
				exp.AddDetail(valSrcExpls[i]);
			}
			return exp;
		}
		
		/// <summary> Explain the custom score.
        /// The doc is relative to the current reader, which is
        /// unknown to CustomScoreQuery when using per-segment search (since Lucene 2.9).
        /// Please override {@link #getCustomScoreProvider} and return a subclass
        /// of {@link CustomScoreProvider} for the given {@link IndexReader}.
		/// </summary>
        [Obsolete("Will be removed in Lucene 3.1")]
		public virtual Explanation CustomExplain(int doc, Explanation subQueryExpl, Explanation valSrcExpl)
		{
			float valSrcScore = 1;
			if (valSrcExpl != null)
			{
				valSrcScore *= valSrcExpl.GetValue();
			}
			Explanation exp = new Explanation(valSrcScore * subQueryExpl.GetValue(), "custom score: product of:");
			exp.AddDetail(subQueryExpl);
			exp.AddDetail(valSrcExpl);
			return exp;
		}
		
		//=========================== W E I G H T ============================
		
		[Serializable]
		private class CustomWeight:Weight
		{
			private void  InitBlock(CustomScoreQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private CustomScoreQuery enclosingInstance;
			public CustomScoreQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal Similarity similarity;
			internal Weight subQueryWeight;
			internal Weight[] valSrcWeights;
			internal bool qStrict;
			
			public CustomWeight(CustomScoreQuery enclosingInstance, Searcher searcher)
			{
				InitBlock(enclosingInstance);
				this.similarity = Enclosing_Instance.GetSimilarity(searcher);
				this.subQueryWeight = Enclosing_Instance.subQuery.Weight(searcher);
				this.valSrcWeights = new Weight[Enclosing_Instance.valSrcQueries.Length];
				for (int i = 0; i < Enclosing_Instance.valSrcQueries.Length; i++)
				{
					this.valSrcWeights[i] = Enclosing_Instance.valSrcQueries[i].CreateWeight(searcher);
				}
				this.qStrict = Enclosing_Instance.strict;
			}
			
			/*(non-Javadoc) @see Mono.Lucene.Net.Search.Weight#getQuery() */
			public override Query GetQuery()
			{
				return Enclosing_Instance;
			}
			
			/*(non-Javadoc) @see Mono.Lucene.Net.Search.Weight#getValue() */
			public override float GetValue()
			{
				return Enclosing_Instance.GetBoost();
			}
			
			/*(non-Javadoc) @see Mono.Lucene.Net.Search.Weight#sumOfSquaredWeights() */
			public override float SumOfSquaredWeights()
			{
				float sum = subQueryWeight.SumOfSquaredWeights();
				for (int i = 0; i < valSrcWeights.Length; i++)
				{
					if (qStrict)
					{
						valSrcWeights[i].SumOfSquaredWeights(); // do not include ValueSource part in the query normalization
					}
					else
					{
						sum += valSrcWeights[i].SumOfSquaredWeights();
					}
				}
				sum *= Enclosing_Instance.GetBoost() * Enclosing_Instance.GetBoost(); // boost each sub-weight
				return sum;
			}
			
			/*(non-Javadoc) @see Mono.Lucene.Net.Search.Weight#normalize(float) */
			public override void  Normalize(float norm)
			{
				norm *= Enclosing_Instance.GetBoost(); // incorporate boost
				subQueryWeight.Normalize(norm);
				for (int i = 0; i < valSrcWeights.Length; i++)
				{
					if (qStrict)
					{
						valSrcWeights[i].Normalize(1); // do not normalize the ValueSource part
					}
					else
					{
						valSrcWeights[i].Normalize(norm);
					}
				}
			}
			
			public override Scorer Scorer(IndexReader reader, bool scoreDocsInOrder, bool topScorer)
			{
				// Pass true for "scoresDocsInOrder", because we
				// require in-order scoring, even if caller does not,
				// since we call advance on the valSrcScorers.  Pass
				// false for "topScorer" because we will not invoke
				// score(Collector) on these scorers:
				Scorer subQueryScorer = subQueryWeight.Scorer(reader, true, false);
				if (subQueryScorer == null)
				{
					return null;
				}
				Scorer[] valSrcScorers = new Scorer[valSrcWeights.Length];
				for (int i = 0; i < valSrcScorers.Length; i++)
				{
					valSrcScorers[i] = valSrcWeights[i].Scorer(reader, true, topScorer);
				}
				return new CustomScorer(enclosingInstance, similarity, reader, this, subQueryScorer, valSrcScorers);
			}
			
			public override Explanation Explain(IndexReader reader, int doc)
			{
				Explanation explain = DoExplain(reader, doc);
				return explain == null?new Explanation(0.0f, "no matching docs"):explain;
			}
			
			private Explanation DoExplain(IndexReader reader, int doc)
			{
				Scorer[] valSrcScorers = new Scorer[valSrcWeights.Length];
				for (int i = 0; i < valSrcScorers.Length; i++)
				{
					valSrcScorers[i] = valSrcWeights[i].Scorer(reader, true, false);
				}
				Explanation subQueryExpl = subQueryWeight.Explain(reader, doc);
				if (!subQueryExpl.IsMatch())
				{
					return subQueryExpl;
				}
				// match
				Explanation[] valSrcExpls = new Explanation[valSrcScorers.Length];
				for (int i = 0; i < valSrcScorers.Length; i++)
				{
					valSrcExpls[i] = valSrcScorers[i].Explain(doc);
				}
                Explanation customExp = Enclosing_Instance.GetCustomScoreProvider(reader).CustomExplain(doc, subQueryExpl, valSrcExpls);
				float sc = GetValue() * customExp.GetValue();
				Explanation res = new ComplexExplanation(true, sc, Enclosing_Instance.ToString() + ", product of:");
				res.AddDetail(customExp);
				res.AddDetail(new Explanation(GetValue(), "queryBoost")); // actually using the q boost as q weight (== weight value)
				return res;
			}
			
			public override bool ScoresDocsOutOfOrder()
			{
				return false;
			}
		}
		
		
		//=========================== S C O R E R ============================
		
		/// <summary> A scorer that applies a (callback) function on scores of the subQuery.</summary>
		private class CustomScorer:Scorer
		{
			private void  InitBlock(CustomScoreQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private CustomScoreQuery enclosingInstance;
			public CustomScoreQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private CustomWeight weight;
			private float qWeight;
			private Scorer subQueryScorer;
			private Scorer[] valSrcScorers;
			private IndexReader reader;
            private CustomScoreProvider provider;
			private float[] vScores; // reused in score() to avoid allocating this array for each doc 
			
			// constructor
			internal CustomScorer(CustomScoreQuery enclosingInstance, Similarity similarity, IndexReader reader, CustomWeight w, Scorer subQueryScorer, Scorer[] valSrcScorers):base(similarity)
			{
				InitBlock(enclosingInstance);
				this.weight = w;
				this.qWeight = w.GetValue();
				this.subQueryScorer = subQueryScorer;
				this.valSrcScorers = valSrcScorers;
				this.reader = reader;
				this.vScores = new float[valSrcScorers.Length];
                this.provider = this.Enclosing_Instance.GetCustomScoreProvider(reader);
			}
			
			/// <deprecated> use {@link #NextDoc()} instead. 
			/// </deprecated>
            [Obsolete("use NextDoc() instead.")]
			public override bool Next()
			{
				return NextDoc() != NO_MORE_DOCS;
			}
			
			public override int NextDoc()
			{
				int doc = subQueryScorer.NextDoc();
				if (doc != NO_MORE_DOCS)
				{
					for (int i = 0; i < valSrcScorers.Length; i++)
					{
						valSrcScorers[i].Advance(doc);
					}
				}
				return doc;
			}
			
			/// <deprecated> use {@link #DocID()} instead. 
			/// </deprecated>
            [Obsolete("use DocID() instead.")]
			public override int Doc()
			{
				return subQueryScorer.Doc();
			}
			
			public override int DocID()
			{
				return subQueryScorer.DocID();
			}
			
			/*(non-Javadoc) @see Mono.Lucene.Net.Search.Scorer#score() */
			public override float Score()
			{
				for (int i = 0; i < valSrcScorers.Length; i++)
				{
					vScores[i] = valSrcScorers[i].Score();
				}
                return qWeight * provider.CustomScore(subQueryScorer.DocID(), subQueryScorer.Score(), vScores);
			}
			
			/// <deprecated> use {@link #Advance(int)} instead. 
			/// </deprecated>
            [Obsolete("use Advance(int) instead.")]
			public override bool SkipTo(int target)
			{
				return Advance(target) != NO_MORE_DOCS;
			}
			
			public override int Advance(int target)
			{
				int doc = subQueryScorer.Advance(target);
				if (doc != NO_MORE_DOCS)
				{
					for (int i = 0; i < valSrcScorers.Length; i++)
					{
						valSrcScorers[i].Advance(doc);
					}
				}
				return doc;
			}
			
			// TODO: remove in 3.0
			/*(non-Javadoc) @see Mono.Lucene.Net.Search.Scorer#explain(int) */
			public override Explanation Explain(int doc)
			{
				Explanation subQueryExpl = weight.subQueryWeight.Explain(reader, doc);
				if (!subQueryExpl.IsMatch())
				{
					return subQueryExpl;
				}
				// match
				Explanation[] valSrcExpls = new Explanation[valSrcScorers.Length];
				for (int i = 0; i < valSrcScorers.Length; i++)
				{
					valSrcExpls[i] = valSrcScorers[i].Explain(doc);
				}
				Explanation customExp = Enclosing_Instance.CustomExplain(doc, subQueryExpl, valSrcExpls);
				float sc = qWeight * customExp.GetValue();
				Explanation res = new ComplexExplanation(true, sc, Enclosing_Instance.ToString() + ", product of:");
				res.AddDetail(customExp);
				res.AddDetail(new Explanation(qWeight, "queryBoost")); // actually using the q boost as q weight (== weight value)
				return res;
			}
		}
		
		public override Weight CreateWeight(Searcher searcher)
		{
			return new CustomWeight(this, searcher);
		}
		
		/// <summary> Checks if this is strict custom scoring.
		/// In strict custom scoring, the ValueSource part does not participate in weight normalization.
		/// This may be useful when one wants full control over how scores are modified, and does 
		/// not care about normalizing by the ValueSource part.
		/// One particular case where this is useful if for testing this query.   
		/// <p/>
		/// Note: only has effect when the ValueSource part is not null.
		/// </summary>
		public virtual bool IsStrict()
		{
			return strict;
		}
		
		/// <summary> Set the strict mode of this query. </summary>
		/// <param name="strict">The strict mode to set.
		/// </param>
		/// <seealso cref="IsStrict()">
		/// </seealso>
		public virtual void  SetStrict(bool strict)
		{
			this.strict = strict;
		}
		
		/// <summary> A short name of this query, used in {@link #ToString(String)}.</summary>
		public virtual System.String Name()
		{
			return "custom";
		}
	}
}
