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
using TermDocs = Monodoc.Lucene.Net.Index.TermDocs;
namespace Monodoc.Lucene.Net.Search
{
	
	sealed class TermScorer:Scorer
	{
		private Weight weight;
		private TermDocs termDocs;
		private byte[] norms;
		private float weightValue;
		private int doc;
		
		private int[] docs = new int[32]; // buffered doc numbers
		private int[] freqs = new int[32]; // buffered term freqs
		private int pointer;
		private int pointerMax;
		
		private const int SCORE_CACHE_SIZE = 32;
		private float[] scoreCache = new float[SCORE_CACHE_SIZE];
		
		internal TermScorer(Weight weight, TermDocs td, Similarity similarity, byte[] norms) : base(similarity)
		{
			this.weight = weight;
			this.termDocs = td;
			this.norms = norms;
			this.weightValue = weight.Value;
			
			for (int i = 0; i < SCORE_CACHE_SIZE; i++)
				scoreCache[i] = GetSimilarity().Tf(i) * weightValue;
		}
		
		public override int Doc()
		{
			return doc;
		}
		
		public override bool Next()
		{
			pointer++;
			if (pointer >= pointerMax)
			{
				pointerMax = termDocs.Read(docs, freqs); // refill buffer
				if (pointerMax != 0)
				{
					pointer = 0;
				}
				else
				{
					termDocs.Close(); // close stream
					doc = System.Int32.MaxValue; // set to sentinel value
					return false;
				}
			}
			doc = docs[pointer];
			return true;
		}
		
		public override float Score()
		{
			int f = freqs[pointer];
			float raw = f < SCORE_CACHE_SIZE ? scoreCache[f] : GetSimilarity().Tf(f) * weightValue; // cache miss
			
			return raw * Similarity.DecodeNorm(norms[doc]); // normalize for Field
		}
		
		public override bool SkipTo(int target)
		{
			// first scan in cache
			for (pointer++; pointer < pointerMax; pointer++)
			{
				if (docs[pointer] >= target)
				{
					doc = docs[pointer];
					return true;
				}
			}
			
			// not found in cache, seek underlying stream
			bool result = termDocs.SkipTo(target);
			if (result)
			{
				pointerMax = 1;
				pointer = 0;
				docs[pointer] = doc = termDocs.Doc();
				freqs[pointer] = termDocs.Freq();
			}
			else
			{
				doc = System.Int32.MaxValue;
			}
			return result;
		}
		
		public override Explanation Explain(int doc)
		{
			TermQuery query = (TermQuery) weight.Query;
			Explanation tfExplanation = new Explanation();
			int tf = 0;
			while (pointer < pointerMax)
			{
				if (docs[pointer] == doc)
					tf = freqs[pointer];
				pointer++;
			}
			if (tf == 0)
			{
				while (termDocs.Next())
				{
					if (termDocs.Doc() == doc)
					{
						tf = termDocs.Freq();
					}
				}
			}
			termDocs.Close();
			tfExplanation.SetValue(GetSimilarity().Tf(tf));
			tfExplanation.SetDescription("tf(termFreq(" + query.GetTerm() + ")=" + tf + ")");
			
			return tfExplanation;
		}
		
		public override System.String ToString()
		{
			return "scorer(" + weight + ")";
		}
	}
}