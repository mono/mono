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
	
	abstract class PhraseScorer:Scorer
	{
		private Weight weight;
		protected internal byte[] norms;
		protected internal float value_Renamed;
		
		private bool firstTime = true;
		private bool more = true;
		protected internal PhraseQueue pq;
		protected internal PhrasePositions first, last;
		
		private float freq;
		
		
		internal PhraseScorer(Weight weight, TermPositions[] tps, int[] positions, Similarity similarity, byte[] norms) : base(similarity)
		{
			this.norms = norms;
			this.weight = weight;
			this.value_Renamed = weight.Value;
			
			// convert tps to a list
			for (int i = 0; i < tps.Length; i++)
			{
				PhrasePositions pp = new PhrasePositions(tps[i], positions[i]);
				if (last != null)
				{
					// add next to end of list
					last.next = pp;
				}
				else
					first = pp;
				last = pp;
			}
			
			pq = new PhraseQueue(tps.Length); // construct empty pq
		}
		
		public override int Doc()
		{
			return first.doc;
		}
		
		public override bool Next()
		{
			if (firstTime)
			{
				Init();
				firstTime = false;
			}
			else if (more)
			{
				more = last.Next(); // trigger further scanning
			}
			return DoNext();
		}
		
		// next without initial increment
		private bool DoNext()
		{
			while (more)
			{
				while (more && first.doc < last.doc)
				{
					// find doc w/ all the terms
					more = first.SkipTo(last.doc); // skip first upto last
					FirstToLast(); // and move it to the end
				}
				
				if (more)
				{
					// found a doc with all of the terms
					freq = PhraseFreq(); // check for phrase
					if (freq == 0.0f)
					// no match
						more = last.Next();
					// trigger further scanning
					else
						return true; // found a match
				}
			}
			return false; // no more matches
		}
		
		public override float Score()
		{
			//System.out.println("scoring " + first.doc);
			float raw = GetSimilarity().Tf(freq) * value_Renamed; // raw score
			return raw * Similarity.DecodeNorm(norms[first.doc]); // normalize
		}
		
		public override bool SkipTo(int target)
		{
			for (PhrasePositions pp = first; more && pp != null; pp = pp.next)
			{
				more = pp.SkipTo(target);
			}
			if (more)
				Sort(); // re-sort
			return DoNext();
		}
		
		protected internal abstract float PhraseFreq();
		
		private void  Init()
		{
			for (PhrasePositions pp = first; more && pp != null; pp = pp.next)
				more = pp.Next();
			if (more)
				Sort();
		}
		
		private void  Sort()
		{
			pq.Clear();
			for (PhrasePositions pp = first; pp != null; pp = pp.next)
				pq.Put(pp);
			PqToList();
		}
		
		protected internal void  PqToList()
		{
			last = first = null;
			while (pq.Top() != null)
			{
				PhrasePositions pp = (PhrasePositions) pq.Pop();
				if (last != null)
				{
					// add next to end of list
					last.next = pp;
				}
				else
					first = pp;
				last = pp;
				pp.next = null;
			}
		}
		
		protected internal void  FirstToLast()
		{
			last.next = first; // move first to end of list
			last = first;
			first = first.next;
			last.next = null;
		}
		
		public override Explanation Explain(int doc)
		{
			Explanation tfExplanation = new Explanation();
			
			while (Next() && Doc() < doc)
			{
			}
			
			float phraseFreq = (Doc() == doc)?freq:0.0f;
			tfExplanation.SetValue(GetSimilarity().Tf(phraseFreq));
			tfExplanation.SetDescription("tf(phraseFreq=" + phraseFreq + ")");
			
			return tfExplanation;
		}
		
		public override System.String ToString()
		{
			return "scorer(" + weight + ")";
		}
	}
}