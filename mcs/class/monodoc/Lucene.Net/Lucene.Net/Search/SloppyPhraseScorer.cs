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
using TermPositions = Monodoc.Lucene.Net.Index.TermPositions;
namespace Monodoc.Lucene.Net.Search
{
	
	sealed class SloppyPhraseScorer:PhraseScorer
	{
		private int slop;
		
		internal SloppyPhraseScorer(Weight weight, TermPositions[] tps, int[] positions, Similarity similarity, int slop, byte[] norms) : base(weight, tps, positions, similarity, norms)
		{
			this.slop = slop;
		}
		
		protected internal override float PhraseFreq()
		{
			pq.Clear();
			int end = 0;
			for (PhrasePositions pp = first; pp != null; pp = pp.next)
			{
				pp.FirstPosition();
				if (pp.position > end)
					end = pp.position;
				pq.Put(pp); // build pq from list
			}
			
			float freq = 0.0f;
			bool done = false;
			do 
			{
				PhrasePositions pp = (PhrasePositions) pq.Pop();
				int start = pp.position;
				int next = ((PhrasePositions) pq.Top()).position;
				for (int pos = start; pos <= next; pos = pp.position)
				{
					start = pos; // advance pp to min window
					if (!pp.NextPosition())
					{
						done = true; // ran out of a term -- done
						break;
					}
				}
				
				int matchLength = end - start;
				if (matchLength <= slop)
					freq += GetSimilarity().SloppyFreq(matchLength); // score match
				
				if (pp.position > end)
					end = pp.position;
				pq.Put(pp); // restore pq
			}
			while (!done);
			
			return freq;
		}
	}
}