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

using ScorerDocQueue = Mono.Lucene.Net.Util.ScorerDocQueue;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary>A Scorer for OR like queries, counterpart of <code>ConjunctionScorer</code>.
	/// This Scorer implements {@link Scorer#SkipTo(int)} and uses skipTo() on the given Scorers. 
	/// TODO: Implement score(HitCollector, int).
	/// </summary>
	class DisjunctionSumScorer:Scorer
	{
		/// <summary>The number of subscorers. </summary>
		private int nrScorers;
		
		/// <summary>The subscorers. </summary>
		protected internal System.Collections.IList subScorers;
		
		/// <summary>The minimum number of scorers that should match. </summary>
		private int minimumNrMatchers;
		
		/// <summary>The scorerDocQueue contains all subscorers ordered by their current doc(),
		/// with the minimum at the top.
		/// <br/>The scorerDocQueue is initialized the first time next() or skipTo() is called.
		/// <br/>An exhausted scorer is immediately removed from the scorerDocQueue.
		/// <br/>If less than the minimumNrMatchers scorers
		/// remain in the scorerDocQueue next() and skipTo() return false.
		/// <p/>
		/// After each to call to next() or skipTo()
		/// <code>currentSumScore</code> is the total score of the current matching doc,
		/// <code>nrMatchers</code> is the number of matching scorers,
		/// and all scorers are after the matching doc, or are exhausted.
		/// </summary>
		private ScorerDocQueue scorerDocQueue;
		
		/// <summary>The document number of the current match. </summary>
		private int currentDoc = - 1;
		
		/// <summary>The number of subscorers that provide the current match. </summary>
		protected internal int nrMatchers = - 1;
		
		private float currentScore = System.Single.NaN;
		
		/// <summary>Construct a <code>DisjunctionScorer</code>.</summary>
		/// <param name="subScorers">A collection of at least two subscorers.
		/// </param>
		/// <param name="minimumNrMatchers">The positive minimum number of subscorers that should
		/// match to match this query.
		/// <br/>When <code>minimumNrMatchers</code> is bigger than
		/// the number of <code>subScorers</code>,
		/// no matches will be produced.
		/// <br/>When minimumNrMatchers equals the number of subScorers,
		/// it more efficient to use <code>ConjunctionScorer</code>.
		/// </param>
		public DisjunctionSumScorer(System.Collections.IList subScorers, int minimumNrMatchers):base(null)
		{
			
			nrScorers = subScorers.Count;
			
			if (minimumNrMatchers <= 0)
			{
				throw new System.ArgumentException("Minimum nr of matchers must be positive");
			}
			if (nrScorers <= 1)
			{
				throw new System.ArgumentException("There must be at least 2 subScorers");
			}
			
			this.minimumNrMatchers = minimumNrMatchers;
			this.subScorers = subScorers;
			
			InitScorerDocQueue();
		}
		
		/// <summary>Construct a <code>DisjunctionScorer</code>, using one as the minimum number
		/// of matching subscorers.
		/// </summary>
		public DisjunctionSumScorer(System.Collections.IList subScorers):this(subScorers, 1)
		{
		}
		
		/// <summary>Called the first time next() or skipTo() is called to
		/// initialize <code>scorerDocQueue</code>.
		/// </summary>
		private void  InitScorerDocQueue()
		{
			System.Collections.IEnumerator si = subScorers.GetEnumerator();
			scorerDocQueue = new ScorerDocQueue(nrScorers);
			while (si.MoveNext())
			{
				Scorer se = (Scorer) si.Current;
				if (se.NextDoc() != NO_MORE_DOCS)
				{
					// doc() method will be used in scorerDocQueue.
					scorerDocQueue.Insert(se);
				}
			}
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
			while (NextDoc() != NO_MORE_DOCS)
			{
				collector.Collect(currentDoc);
			}
		}
		
		/// <summary>Expert: Collects matching documents in a range.  Hook for optimization.
		/// Note that {@link #Next()} must be called once before this method is called
		/// for the first time.
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
		
		/// <summary>Expert: Collects matching documents in a range.  Hook for optimization.
		/// Note that {@link #Next()} must be called once before this method is called
		/// for the first time.
		/// </summary>
		/// <param name="collector">The collector to which all matching documents are passed through.
		/// </param>
		/// <param name="max">Do not score documents past this.
		/// </param>
		/// <returns> true if more matching documents may remain.
		/// </returns>
		public /*protected internal*/ override bool Score(Collector collector, int max, int firstDocID)
		{
			// firstDocID is ignored since nextDoc() sets 'currentDoc'
			collector.SetScorer(this);
			while (currentDoc < max)
			{
				collector.Collect(currentDoc);
				if (NextDoc() == NO_MORE_DOCS)
				{
					return false;
				}
			}
			return true;
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
			if (scorerDocQueue.Size() < minimumNrMatchers || !AdvanceAfterCurrent())
			{
				currentDoc = NO_MORE_DOCS;
			}
			return currentDoc;
		}
		
		/// <summary>Advance all subscorers after the current document determined by the
		/// top of the <code>scorerDocQueue</code>.
		/// Repeat until at least the minimum number of subscorers match on the same
		/// document and all subscorers are after that document or are exhausted.
		/// <br/>On entry the <code>scorerDocQueue</code> has at least <code>minimumNrMatchers</code>
		/// available. At least the scorer with the minimum document number will be advanced.
		/// </summary>
		/// <returns> true iff there is a match.
		/// <br/>In case there is a match, <code>currentDoc</code>, <code>currentSumScore</code>,
		/// and <code>nrMatchers</code> describe the match.
		/// 
		/// TODO: Investigate whether it is possible to use skipTo() when
		/// the minimum number of matchers is bigger than one, ie. try and use the
		/// character of ConjunctionScorer for the minimum number of matchers.
		/// Also delay calling score() on the sub scorers until the minimum number of
		/// matchers is reached.
		/// <br/>For this, a Scorer array with minimumNrMatchers elements might
		/// hold Scorers at currentDoc that are temporarily popped from scorerQueue.
		/// </returns>
		protected internal virtual bool AdvanceAfterCurrent()
		{
			do 
			{
				// repeat until minimum nr of matchers
				currentDoc = scorerDocQueue.TopDoc();
				currentScore = scorerDocQueue.TopScore();
				nrMatchers = 1;
				do 
				{
					// Until all subscorers are after currentDoc
					if (!scorerDocQueue.TopNextAndAdjustElsePop())
					{
						if (scorerDocQueue.Size() == 0)
						{
							break; // nothing more to advance, check for last match.
						}
					}
					if (scorerDocQueue.TopDoc() != currentDoc)
					{
						break; // All remaining subscorers are after currentDoc.
					}
					currentScore += scorerDocQueue.TopScore();
					nrMatchers++;
				}
				while (true);
				
				if (nrMatchers >= minimumNrMatchers)
				{
					return true;
				}
				else if (scorerDocQueue.Size() < minimumNrMatchers)
				{
					return false;
				}
			}
			while (true);
		}
		
		/// <summary>Returns the score of the current document matching the query.
		/// Initially invalid, until {@link #Next()} is called the first time.
		/// </summary>
		public override float Score()
		{
			return currentScore;
		}
		
		/// <deprecated> use {@link #DocID()} instead. 
		/// </deprecated>
        [Obsolete("use DocID() instead. ")]
		public override int Doc()
		{
			return currentDoc;
		}
		
		public override int DocID()
		{
			return currentDoc;
		}
		
		/// <summary>Returns the number of subscorers matching the current document.
		/// Initially invalid, until {@link #Next()} is called the first time.
		/// </summary>
		public virtual int NrMatchers()
		{
			return nrMatchers;
		}
		
		/// <summary> Skips to the first match beyond the current whose document number is
		/// greater than or equal to a given target. <br/>
		/// When this method is used the {@link #Explain(int)} method should not be
		/// used. <br/>
		/// The implementation uses the skipTo() method on the subscorers.
		/// 
		/// </summary>
		/// <param name="target">The target document number.
		/// </param>
		/// <returns> true iff there is such a match.
		/// </returns>
		/// <deprecated> use {@link #Advance(int)} instead.
		/// </deprecated>
        [Obsolete("use Advance(int) instead.")]
		public override bool SkipTo(int target)
		{
			return Advance(target) != NO_MORE_DOCS;
		}
		
		/// <summary> Advances to the first match beyond the current whose document number is
		/// greater than or equal to a given target. <br/>
		/// When this method is used the {@link #Explain(int)} method should not be
		/// used. <br/>
		/// The implementation uses the skipTo() method on the subscorers.
		/// 
		/// </summary>
		/// <param name="target">The target document number.
		/// </param>
		/// <returns> the document whose number is greater than or equal to the given
		/// target, or -1 if none exist.
		/// </returns>
		public override int Advance(int target)
		{
			if (scorerDocQueue.Size() < minimumNrMatchers)
			{
				return currentDoc = NO_MORE_DOCS;
			}
			if (target <= currentDoc)
			{
				return currentDoc;
			}
			do 
			{
				if (scorerDocQueue.TopDoc() >= target)
				{
					return AdvanceAfterCurrent()?currentDoc:(currentDoc = NO_MORE_DOCS);
				}
				else if (!scorerDocQueue.TopSkipToAndAdjustElsePop(target))
				{
					if (scorerDocQueue.Size() < minimumNrMatchers)
					{
						return currentDoc = NO_MORE_DOCS;
					}
				}
			}
			while (true);
		}
		
		/// <returns> An explanation for the score of a given document. 
		/// </returns>
		public override Explanation Explain(int doc)
		{
			Explanation res = new Explanation();
			System.Collections.IEnumerator ssi = subScorers.GetEnumerator();
			float sumScore = 0.0f;
			int nrMatches = 0;
			while (ssi.MoveNext())
			{
				Explanation es = ((Scorer) ssi.Current).Explain(doc);
				if (es.GetValue() > 0.0f)
				{
					// indicates match
					sumScore += es.GetValue();
					nrMatches++;
				}
				res.AddDetail(es);
			}
			if (nrMatchers >= minimumNrMatchers)
			{
				res.SetValue(sumScore);
				res.SetDescription("sum over at least " + minimumNrMatchers + " of " + subScorers.Count + ":");
			}
			else
			{
				res.SetValue(0.0f);
				res.SetDescription(nrMatches + " match(es) but at least " + minimumNrMatchers + " of " + subScorers.Count + " needed");
			}
			return res;
		}
	}
}
