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

using Explanation = Mono.Lucene.Net.Search.Explanation;
using Scorer = Mono.Lucene.Net.Search.Scorer;
using Similarity = Mono.Lucene.Net.Search.Similarity;
using Weight = Mono.Lucene.Net.Search.Weight;

namespace Mono.Lucene.Net.Search.Spans
{
	
	/// <summary> Public for extension only.</summary>
	public class SpanScorer:Scorer
	{
		protected internal Spans spans;
		protected internal Weight weight;
		protected internal byte[] norms;
		protected internal float value_Renamed;
		
		/// <deprecated> not needed anymore 
		/// </deprecated>
        [Obsolete("not needed anymore ")]
		protected internal bool firstTime = true;
		protected internal bool more = true;
		
		protected internal int doc;
		protected internal float freq;
		
		protected internal SpanScorer(Spans spans, Weight weight, Similarity similarity, byte[] norms):base(similarity)
		{
			this.spans = spans;
			this.norms = norms;
			this.weight = weight;
			this.value_Renamed = weight.GetValue();
			if (this.spans.Next())
			{
				doc = - 1;
			}
			else
			{
				doc = NO_MORE_DOCS;
				more = false;
			}
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
			if (!SetFreqCurrentDoc())
			{
				doc = NO_MORE_DOCS;
			}
			return doc;
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
			if (!more)
			{
				return doc = NO_MORE_DOCS;
			}
			if (spans.Doc() < target)
			{
				// setFreqCurrentDoc() leaves spans.doc() ahead
				more = spans.SkipTo(target);
			}
			if (!SetFreqCurrentDoc())
			{
				doc = NO_MORE_DOCS;
			}
			return doc;
		}
		
		public /*protected internal*/ virtual bool SetFreqCurrentDoc()
		{
			if (!more)
			{
				return false;
			}
			doc = spans.Doc();
			freq = 0.0f;
			do 
			{
				int matchLength = spans.End() - spans.Start();
				freq += GetSimilarity().SloppyFreq(matchLength);
				more = spans.Next();
			}
			while (more && (doc == spans.Doc()));
			return true;
		}
		
		/// <deprecated> use {@link #DocID()} instead. 
		/// </deprecated>
        [Obsolete("use DocID() instead. ")]
		public override int Doc()
		{
			return doc;
		}
		
		public override int DocID()
		{
			return doc;
		}
		
		public override float Score()
		{
			float raw = GetSimilarity().Tf(freq) * value_Renamed; // raw score
			return norms == null?raw:raw * Similarity.DecodeNorm(norms[doc]); // normalize
		}
		
		public override Explanation Explain(int doc)
		{
			Explanation tfExplanation = new Explanation();
			
			int expDoc = Advance(doc);
			
			float phraseFreq = (expDoc == doc)?freq:0.0f;
			tfExplanation.SetValue(GetSimilarity().Tf(phraseFreq));
			tfExplanation.SetDescription("tf(phraseFreq=" + phraseFreq + ")");
			
			return tfExplanation;
		}
	}
}
