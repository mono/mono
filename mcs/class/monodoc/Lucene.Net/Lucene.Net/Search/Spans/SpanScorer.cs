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
using Explanation = Monodoc.Lucene.Net.Search.Explanation;
using Scorer = Monodoc.Lucene.Net.Search.Scorer;
using Similarity = Monodoc.Lucene.Net.Search.Similarity;
using Weight = Monodoc.Lucene.Net.Search.Weight;
namespace Monodoc.Lucene.Net.Search.Spans
{
	
	
	class SpanScorer:Scorer
	{
		private Spans spans;
		private Weight weight;
		private byte[] norms;
		private float value_Renamed;
		
		private bool firstTime = true;
		private bool more = true;
		
		private int doc;
		private float freq;
		
		internal SpanScorer(Spans spans, Weight weight, Similarity similarity, byte[] norms) : base(similarity)
		{
			this.spans = spans;
			this.norms = norms;
			this.weight = weight;
			this.value_Renamed = weight.Value;
		}
		
		public override bool Next()
		{
			if (firstTime)
			{
				more = spans.Next();
				firstTime = false;
			}
			
			if (!more)
				return false;
			
			freq = 0.0f;
			doc = spans.Doc();
			
			while (more && doc == spans.Doc())
			{
				int matchLength = spans.End() - spans.Start();
				freq += GetSimilarity().SloppyFreq(matchLength);
				more = spans.Next();
			}
			
			return more || freq != 0.0f;
		}
		
		public override int Doc()
		{
			return doc;
		}
		
		public override float Score()
		{
			float raw = GetSimilarity().Tf(freq) * value_Renamed; // raw score
			return raw * Similarity.DecodeNorm(norms[doc]); // normalize
		}
		
		public override bool SkipTo(int target)
		{
			more = spans.SkipTo(target);
			
			if (!more)
				return false;
			
			freq = 0.0f;
			doc = spans.Doc();
			
			while (more && spans.Doc() == target)
			{
				freq += GetSimilarity().SloppyFreq(spans.End() - spans.Start());
				more = spans.Next();
			}
			
			return more || freq != 0.0f;
		}
		
		public override Explanation Explain(int doc)
		{
			Explanation tfExplanation = new Explanation();
			
			SkipTo(doc);
			
			float phraseFreq = (Doc() == doc)?freq:0.0f;
			tfExplanation.SetValue(GetSimilarity().Tf(phraseFreq));
			tfExplanation.SetDescription("tf(phraseFreq=" + phraseFreq + ")");
			
			return tfExplanation;
		}
	}
}