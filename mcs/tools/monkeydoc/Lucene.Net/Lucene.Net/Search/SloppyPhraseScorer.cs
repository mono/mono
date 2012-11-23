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

using TermPositions = Mono.Lucene.Net.Index.TermPositions;

namespace Mono.Lucene.Net.Search
{
	
	sealed class SloppyPhraseScorer:PhraseScorer
	{
		private int slop;
		private PhrasePositions[] repeats;
		private PhrasePositions[] tmpPos; // for flipping repeating pps.
		private bool checkedRepeats;
		
		internal SloppyPhraseScorer(Weight weight, TermPositions[] tps, int[] offsets, Similarity similarity, int slop, byte[] norms):base(weight, tps, offsets, similarity, norms)
		{
			this.slop = slop;
		}
		
		/// <summary> Score a candidate doc for all slop-valid position-combinations (matches) 
		/// encountered while traversing/hopping the PhrasePositions.
		/// <br/> The score contribution of a match depends on the distance: 
		/// <br/> - highest score for distance=0 (exact match).
		/// <br/> - score gets lower as distance gets higher.
		/// <br/>Example: for query "a b"~2, a document "x a b a y" can be scored twice: 
		/// once for "a b" (distance=0), and once for "b a" (distance=2).
		/// <br/>Possibly not all valid combinations are encountered, because for efficiency  
		/// we always propagate the least PhrasePosition. This allows to base on 
		/// PriorityQueue and move forward faster. 
		/// As result, for example, document "a b c b a"
		/// would score differently for queries "a b c"~4 and "c b a"~4, although 
		/// they really are equivalent. 
		/// Similarly, for doc "a b c b a f g", query "c b"~2 
		/// would get same score as "g f"~2, although "c b"~2 could be matched twice.
		/// We may want to fix this in the future (currently not, for performance reasons).
		/// </summary>
		protected internal override float PhraseFreq()
		{
			int end = InitPhrasePositions();
			
			float freq = 0.0f;
			bool done = (end < 0);
			while (!done)
			{
				PhrasePositions pp = (PhrasePositions) pq.Pop();
				int start = pp.position;
				int next = ((PhrasePositions) pq.Top()).position;
				
				bool tpsDiffer = true;
				for (int pos = start; pos <= next || !tpsDiffer; pos = pp.position)
				{
					if (pos <= next && tpsDiffer)
						start = pos; // advance pp to min window
					if (!pp.NextPosition())
					{
						done = true; // ran out of a term -- done
						break;
					}
					PhrasePositions pp2 = null;
					tpsDiffer = !pp.repeats || (pp2 = TermPositionsDiffer(pp)) == null;
					if (pp2 != null && pp2 != pp)
					{
						pp = Flip(pp, pp2); // flip pp to pp2
					}
				}
				
				int matchLength = end - start;
				if (matchLength <= slop)
					freq += GetSimilarity().SloppyFreq(matchLength); // score match
				
				if (pp.position > end)
					end = pp.position;
				pq.Put(pp); // restore pq
			}
			
			return freq;
		}
		
		// flip pp2 and pp in the queue: pop until finding pp2, insert back all but pp2, insert pp back.
		// assumes: pp!=pp2, pp2 in pq, pp not in pq.
		// called only when there are repeating pps.
		private PhrasePositions Flip(PhrasePositions pp, PhrasePositions pp2)
		{
			int n = 0;
			PhrasePositions pp3;
			//pop until finding pp2
			while ((pp3 = (PhrasePositions) pq.Pop()) != pp2)
			{
				tmpPos[n++] = pp3;
			}
			//insert back all but pp2
			for (n--; n >= 0; n--)
			{
				pq.Insert(tmpPos[n]);
			}
			//insert pp back
			pq.Put(pp);
			return pp2;
		}
		
		/// <summary> Init PhrasePositions in place.
		/// There is a one time initialization for this scorer:
		/// <br/>- Put in repeats[] each pp that has another pp with same position in the doc.
		/// <br/>- Also mark each such pp by pp.repeats = true.
		/// <br/>Later can consult with repeats[] in termPositionsDiffer(pp), making that check efficient.
		/// In particular, this allows to score queries with no repetitions with no overhead due to this computation.
		/// <br/>- Example 1 - query with no repetitions: "ho my"~2
		/// <br/>- Example 2 - query with repetitions: "ho my my"~2
		/// <br/>- Example 3 - query with repetitions: "my ho my"~2
		/// <br/>Init per doc w/repeats in query, includes propagating some repeating pp's to avoid false phrase detection.  
		/// </summary>
		/// <returns> end (max position), or -1 if any term ran out (i.e. done) 
		/// </returns>
		/// <throws>  IOException  </throws>
		private int InitPhrasePositions()
		{
			int end = 0;
			
			// no repeats at all (most common case is also the simplest one)
			if (checkedRepeats && repeats == null)
			{
				// build queue from list
				pq.Clear();
				for (PhrasePositions pp = first; pp != null; pp = pp.next)
				{
					pp.FirstPosition();
					if (pp.position > end)
						end = pp.position;
					pq.Put(pp); // build pq from list
				}
				return end;
			}
			
			// position the pp's
			for (PhrasePositions pp = first; pp != null; pp = pp.next)
				pp.FirstPosition();
			
			// one time initializatin for this scorer
			if (!checkedRepeats)
			{
				checkedRepeats = true;
				// check for repeats
				System.Collections.Hashtable m = null;
				for (PhrasePositions pp = first; pp != null; pp = pp.next)
				{
					int tpPos = pp.position + pp.offset;
					for (PhrasePositions pp2 = pp.next; pp2 != null; pp2 = pp2.next)
					{
						int tpPos2 = pp2.position + pp2.offset;
						if (tpPos2 == tpPos)
						{
							if (m == null)
							{
								m = new System.Collections.Hashtable();
							}
							pp.repeats = true;
							pp2.repeats = true;
							m[pp] = null;
							m[pp2] = null;
						}
					}
				}
				if (m != null)
				{
					repeats = (PhrasePositions[])(new System.Collections.ArrayList(m.Keys).ToArray(typeof(PhrasePositions)));
				}
			}
			
			// with repeats must advance some repeating pp's so they all start with differing tp's       
			if (repeats != null)
			{
				for (int i = 0; i < repeats.Length; i++)
				{
					PhrasePositions pp = repeats[i];
					PhrasePositions pp2;
					while ((pp2 = TermPositionsDiffer(pp)) != null)
					{
						if (!pp2.NextPosition())
						// out of pps that do not differ, advance the pp with higher offset 
							return - 1; // ran out of a term -- done  
					}
				}
			}
			
			// build queue from list
			pq.Clear();
			for (PhrasePositions pp = first; pp != null; pp = pp.next)
			{
				if (pp.position > end)
					end = pp.position;
				pq.Put(pp); // build pq from list
			}
			
			if (repeats != null)
			{
				tmpPos = new PhrasePositions[pq.Size()];
			}
			return end;
		}
		
		/// <summary> We disallow two pp's to have the same TermPosition, thereby verifying multiple occurrences 
		/// in the query of the same word would go elsewhere in the matched doc.
		/// </summary>
		/// <returns> null if differ (i.e. valid) otherwise return the higher offset PhrasePositions
		/// out of the first two PPs found to not differ.
		/// </returns>
		private PhrasePositions TermPositionsDiffer(PhrasePositions pp)
		{
			// efficiency note: a more efficient implementation could keep a map between repeating 
			// pp's, so that if pp1a, pp1b, pp1c are repeats term1, and pp2a, pp2b are repeats 
			// of term2, pp2a would only be checked against pp2b but not against pp1a, pp1b, pp1c. 
			// However this would complicate code, for a rather rare case, so choice is to compromise here.
			int tpPos = pp.position + pp.offset;
			for (int i = 0; i < repeats.Length; i++)
			{
				PhrasePositions pp2 = repeats[i];
				if (pp2 == pp)
					continue;
				int tpPos2 = pp2.position + pp2.offset;
				if (tpPos2 == tpPos)
					return pp.offset > pp2.offset?pp:pp2; // do not differ: return the one with higher offset.
			}
			return null;
		}
	}
}
