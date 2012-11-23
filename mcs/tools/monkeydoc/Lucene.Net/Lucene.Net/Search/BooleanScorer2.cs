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

namespace Mono.Lucene.Net.Search
{
	
	/* See the description in BooleanScorer.java, comparing
	* BooleanScorer & BooleanScorer2 */
	
	/// <summary>An alternative to BooleanScorer that also allows a minimum number
	/// of optional scorers that should match.
	/// <br/>Implements skipTo(), and has no limitations on the numbers of added scorers.
	/// <br/>Uses ConjunctionScorer, DisjunctionScorer, ReqOptScorer and ReqExclScorer.
	/// </summary>
	class BooleanScorer2:Scorer
	{
		private class AnonymousClassDisjunctionSumScorer:DisjunctionSumScorer
		{
			private void  InitBlock(BooleanScorer2 enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private BooleanScorer2 enclosingInstance;
			public BooleanScorer2 Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal AnonymousClassDisjunctionSumScorer(BooleanScorer2 enclosingInstance, System.Collections.IList Param1, int Param2):base(Param1, Param2)
			{
				InitBlock(enclosingInstance);
			}
			private int lastScoredDoc = - 1;
			// Save the score of lastScoredDoc, so that we don't compute it more than
			// once in score().
			private float lastDocScore = System.Single.NaN;
			public override float Score()
			{
				int doc = DocID();
				if (doc >= lastScoredDoc)
				{
					if (doc > lastScoredDoc)
					{
						lastDocScore = base.Score();
						lastScoredDoc = doc;
					}
					Enclosing_Instance.coordinator.nrMatchers += base.nrMatchers;
				}
				return lastDocScore;
			}
		}
		private class AnonymousClassConjunctionScorer:ConjunctionScorer
		{
			private void  InitBlock(int requiredNrMatchers, BooleanScorer2 enclosingInstance)
			{
				this.requiredNrMatchers = requiredNrMatchers;
				this.enclosingInstance = enclosingInstance;
			}
			private int requiredNrMatchers;
			private BooleanScorer2 enclosingInstance;
			public BooleanScorer2 Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal AnonymousClassConjunctionScorer(int requiredNrMatchers, BooleanScorer2 enclosingInstance, Mono.Lucene.Net.Search.Similarity Param1, System.Collections.ICollection Param2):base(Param1, Param2)
			{
				InitBlock(requiredNrMatchers, enclosingInstance);
			}
			private int lastScoredDoc = - 1;
			// Save the score of lastScoredDoc, so that we don't compute it more than
			// once in score().
			private float lastDocScore = System.Single.NaN;
			public override float Score()
			{
				int doc = DocID();
				if (doc >= lastScoredDoc)
				{
					if (doc > lastScoredDoc)
					{
						lastDocScore = base.Score();
						lastScoredDoc = doc;
					}
					Enclosing_Instance.coordinator.nrMatchers += requiredNrMatchers;
				}
				// All scorers match, so defaultSimilarity super.score() always has 1 as
				// the coordination factor.
				// Therefore the sum of the scores of the requiredScorers
				// is used as score.
				return lastDocScore;
			}
		}
		
		private System.Collections.IList requiredScorers;
		private System.Collections.IList optionalScorers;
		private System.Collections.IList prohibitedScorers;
		
		private class Coordinator
		{
			public Coordinator(BooleanScorer2 enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(BooleanScorer2 enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private BooleanScorer2 enclosingInstance;
			public BooleanScorer2 Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal float[] coordFactors = null;
			internal int maxCoord = 0; // to be increased for each non prohibited scorer
			internal int nrMatchers; // to be increased by score() of match counting scorers.
			
			internal virtual void  Init()
			{
				// use after all scorers have been added.
				coordFactors = new float[maxCoord + 1];
				Similarity sim = Enclosing_Instance.GetSimilarity();
				for (int i = 0; i <= maxCoord; i++)
				{
					coordFactors[i] = sim.Coord(i, maxCoord);
				}
			}
		}
		
		private Coordinator coordinator;
		
		/// <summary>The scorer to which all scoring will be delegated,
		/// except for computing and using the coordination factor.
		/// </summary>
		private Scorer countingSumScorer;
		
		/// <summary>The number of optionalScorers that need to match (if there are any) </summary>
		private int minNrShouldMatch;
		
		private int doc = - 1;
		
		/// <summary> Creates a {@link Scorer} with the given similarity and lists of required,
		/// prohibited and optional scorers. In no required scorers are added, at least
		/// one of the optional scorers will have to match during the search.
		/// 
		/// </summary>
		/// <param name="similarity">The similarity to be used.
		/// </param>
		/// <param name="minNrShouldMatch">The minimum number of optional added scorers that should match
		/// during the search. In case no required scorers are added, at least
		/// one of the optional scorers will have to match during the search.
		/// </param>
		/// <param name="required">the list of required scorers.
		/// </param>
		/// <param name="prohibited">the list of prohibited scorers.
		/// </param>
		/// <param name="optional">the list of optional scorers.
		/// </param>
		public BooleanScorer2(Similarity similarity, int minNrShouldMatch, System.Collections.IList required, System.Collections.IList prohibited, System.Collections.IList optional):base(similarity)
		{
			if (minNrShouldMatch < 0)
			{
				throw new System.ArgumentException("Minimum number of optional scorers should not be negative");
			}
			coordinator = new Coordinator(this);
			this.minNrShouldMatch = minNrShouldMatch;
			
			optionalScorers = optional;
			coordinator.maxCoord += optional.Count;
			
			requiredScorers = required;
			coordinator.maxCoord += required.Count;
			
			prohibitedScorers = prohibited;
			
			coordinator.Init();
			countingSumScorer = MakeCountingSumScorer();
		}
		
		/// <summary>Count a scorer as a single match. </summary>
		private class SingleMatchScorer:Scorer
		{
			private void  InitBlock(BooleanScorer2 enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private BooleanScorer2 enclosingInstance;
			public BooleanScorer2 Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private Scorer scorer;
			private int lastScoredDoc = - 1;
			// Save the score of lastScoredDoc, so that we don't compute it more than
			// once in score().
			private float lastDocScore = System.Single.NaN;
			
			internal SingleMatchScorer(BooleanScorer2 enclosingInstance, Scorer scorer):base(scorer.GetSimilarity())
			{
				InitBlock(enclosingInstance);
				this.scorer = scorer;
			}
			public override float Score()
			{
				int doc = DocID();
				if (doc >= lastScoredDoc)
				{
					if (doc > lastScoredDoc)
					{
						lastDocScore = scorer.Score();
						lastScoredDoc = doc;
					}
					Enclosing_Instance.coordinator.nrMatchers++;
				}
				return lastDocScore;
			}
			/// <deprecated> use {@link #DocID()} instead. 
			/// </deprecated>
            [Obsolete("use DocID() instead. ")]
			public override int Doc()
			{
				return scorer.Doc();
			}
			public override int DocID()
			{
				return scorer.DocID();
			}
			/// <deprecated> use {@link #NextDoc()} instead. 
			/// </deprecated>
            [Obsolete("use NextDoc() instead. ")]
			public override bool Next()
			{
				return scorer.NextDoc() != NO_MORE_DOCS;
			}
			public override int NextDoc()
			{
				return scorer.NextDoc();
			}
			/// <deprecated> use {@link #Advance(int)} instead. 
			/// </deprecated>
            [Obsolete("use Advance(int) instead. ")]
			public override bool SkipTo(int docNr)
			{
				return scorer.Advance(docNr) != NO_MORE_DOCS;
			}
			public override int Advance(int target)
			{
				return scorer.Advance(target);
			}
			public override Explanation Explain(int docNr)
			{
				return scorer.Explain(docNr);
			}
		}
		
		private Scorer CountingDisjunctionSumScorer(System.Collections.IList scorers, int minNrShouldMatch)
		{
			// each scorer from the list counted as a single matcher
			return new AnonymousClassDisjunctionSumScorer(this, scorers, minNrShouldMatch);
		}
		
		private static readonly Similarity defaultSimilarity;
		
		private Scorer CountingConjunctionSumScorer(System.Collections.IList requiredScorers)
		{
			// each scorer from the list counted as a single matcher
			int requiredNrMatchers = requiredScorers.Count;
			return new AnonymousClassConjunctionScorer(requiredNrMatchers, this, defaultSimilarity, requiredScorers);
		}
		
		private Scorer DualConjunctionSumScorer(Scorer req1, Scorer req2)
		{
			// non counting.
			return new ConjunctionScorer(defaultSimilarity, new Scorer[]{req1, req2});
			// All scorers match, so defaultSimilarity always has 1 as
			// the coordination factor.
			// Therefore the sum of the scores of two scorers
			// is used as score.
		}
		
		/// <summary>Returns the scorer to be used for match counting and score summing.
		/// Uses requiredScorers, optionalScorers and prohibitedScorers.
		/// </summary>
		private Scorer MakeCountingSumScorer()
		{
			// each scorer counted as a single matcher
			return (requiredScorers.Count == 0)?MakeCountingSumScorerNoReq():MakeCountingSumScorerSomeReq();
		}
		
		private Scorer MakeCountingSumScorerNoReq()
		{
			// No required scorers
			// minNrShouldMatch optional scorers are required, but at least 1
			int nrOptRequired = (minNrShouldMatch < 1)?1:minNrShouldMatch;
			Scorer requiredCountingSumScorer;
			if (optionalScorers.Count > nrOptRequired)
				requiredCountingSumScorer = CountingDisjunctionSumScorer(optionalScorers, nrOptRequired);
			else if (optionalScorers.Count == 1)
				requiredCountingSumScorer = new SingleMatchScorer(this, (Scorer) optionalScorers[0]);
			else
				requiredCountingSumScorer = CountingConjunctionSumScorer(optionalScorers);
			return AddProhibitedScorers(requiredCountingSumScorer);
		}
		
		private Scorer MakeCountingSumScorerSomeReq()
		{
			// At least one required scorer.
			if (optionalScorers.Count == minNrShouldMatch)
			{
				// all optional scorers also required.
				System.Collections.ArrayList allReq = new System.Collections.ArrayList(requiredScorers);
				allReq.AddRange(optionalScorers);
				return AddProhibitedScorers(CountingConjunctionSumScorer(allReq));
			}
			else
			{
				// optionalScorers.size() > minNrShouldMatch, and at least one required scorer
				Scorer requiredCountingSumScorer = requiredScorers.Count == 1?new SingleMatchScorer(this, (Scorer) requiredScorers[0]):CountingConjunctionSumScorer(requiredScorers);
				if (minNrShouldMatch > 0)
				{
					// use a required disjunction scorer over the optional scorers
					return AddProhibitedScorers(DualConjunctionSumScorer(requiredCountingSumScorer, CountingDisjunctionSumScorer(optionalScorers, minNrShouldMatch)));
				}
				else
				{
					// minNrShouldMatch == 0
					return new ReqOptSumScorer(AddProhibitedScorers(requiredCountingSumScorer), optionalScorers.Count == 1?new SingleMatchScorer(this, (Scorer) optionalScorers[0]):CountingDisjunctionSumScorer(optionalScorers, 1));
				}
			}
		}
		
		/// <summary>Returns the scorer to be used for match counting and score summing.
		/// Uses the given required scorer and the prohibitedScorers.
		/// </summary>
		/// <param name="requiredCountingSumScorer">A required scorer already built.
		/// </param>
		private Scorer AddProhibitedScorers(Scorer requiredCountingSumScorer)
		{
			return (prohibitedScorers.Count == 0)?requiredCountingSumScorer:new ReqExclScorer(requiredCountingSumScorer, ((prohibitedScorers.Count == 1)?(Scorer) prohibitedScorers[0]:new DisjunctionSumScorer(prohibitedScorers)));
		}
		
		/// <summary>Scores and collects all matching documents.</summary>
		/// <param name="hc">The collector to which all matching documents are passed through
		/// {@link HitCollector#Collect(int, float)}.
		/// <br/>When this method is used the {@link #Explain(int)} method should not be used.
		/// </param>
		/// <deprecated> use {@link #Score(Collector)} instead.
		/// </deprecated>
        [Obsolete("use Score(Collector) instead.")]
		public override void  Score(HitCollector hc)
		{
			Score(new HitCollectorWrapper(hc));
		}
		
		/// <summary>Scores and collects all matching documents.</summary>
		/// <param name="collector">The collector to which all matching documents are passed through.
		/// <br/>When this method is used the {@link #Explain(int)} method should not be used.
		/// </param>
		public override void  Score(Collector collector)
		{
			collector.SetScorer(this);
			while ((doc = countingSumScorer.NextDoc()) != NO_MORE_DOCS)
			{
				collector.Collect(doc);
			}
		}
		
		/// <summary>Expert: Collects matching documents in a range.
		/// <br/>Note that {@link #Next()} must be called once before this method is
		/// called for the first time.
		/// </summary>
		/// <param name="hc">The collector to which all matching documents are passed through
		/// {@link HitCollector#Collect(int, float)}.
		/// </param>
		/// <param name="max">Do not score documents past this.
		/// </param>
		/// <returns> true if more matching documents may remain.
		/// </returns>
		/// <deprecated> use {@link #Score(Collector, int, int)} instead.
		/// </deprecated>
        [Obsolete("use Score(Collector, int, int) instead.")]
		protected internal override bool Score(HitCollector hc, int max)
		{
			return Score(new HitCollectorWrapper(hc), max, DocID());
		}
		
		public /*protected internal*/ override bool Score(Collector collector, int max, int firstDocID)
		{
			doc = firstDocID;
			collector.SetScorer(this);
			while (doc < max)
			{
				collector.Collect(doc);
				doc = countingSumScorer.NextDoc();
			}
			return doc != NO_MORE_DOCS;
		}
		
		/// <deprecated> use {@link #DocID()} instead. 
		/// </deprecated>
        [Obsolete("use DocID() instead. ")]
		public override int Doc()
		{
			return countingSumScorer.Doc();
		}
		
		public override int DocID()
		{
			return doc;
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
			return doc = countingSumScorer.NextDoc();
		}
		
		public override float Score()
		{
			coordinator.nrMatchers = 0;
			float sum = countingSumScorer.Score();
			return sum * coordinator.coordFactors[coordinator.nrMatchers];
		}
		
		/// <deprecated> use {@link #Advance(int)} instead. 
		/// </deprecated>
        [Obsolete("use Advance(int) instead. ")]
		public override bool SkipTo(int target)
		{
			return Advance(target) != NO_MORE_DOCS;
		}
		
		public override int Advance(int target)
		{
			return doc = countingSumScorer.Advance(target);
		}
		
		/// <summary>Throws an UnsupportedOperationException.
		/// TODO: Implement an explanation of the coordination factor.
		/// </summary>
		/// <param name="doc">The document number for the explanation.
		/// </param>
		/// <throws>  UnsupportedOperationException </throws>
		public override Explanation Explain(int doc)
		{
			throw new System.NotSupportedException();
			/* How to explain the coordination factor?
			initCountingSumScorer();
			return countingSumScorer.explain(doc); // misses coord factor. 
			*/
		}
		static BooleanScorer2()
		{
			defaultSimilarity = Similarity.GetDefault();
		}
	}
}
