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

using TermDocs = Mono.Lucene.Net.Index.TermDocs;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary>Expert: A <code>Scorer</code> for documents matching a <code>Term</code>.</summary>
	public sealed class TermScorer:Scorer
	{
		
		private static readonly float[] SIM_NORM_DECODER;
		
		private Weight weight;
		private TermDocs termDocs;
		private byte[] norms;
		private float weightValue;
		private int doc = - 1;
		
		private int[] docs = new int[32]; // buffered doc numbers
		private int[] freqs = new int[32]; // buffered term freqs
		private int pointer;
		private int pointerMax;
		
		private const int SCORE_CACHE_SIZE = 32;
		private float[] scoreCache = new float[SCORE_CACHE_SIZE];
		
		/// <summary> Construct a <code>TermScorer</code>.
		/// 
		/// </summary>
		/// <param name="weight">The weight of the <code>Term</code> in the query.
		/// </param>
		/// <param name="td">An iterator over the documents matching the <code>Term</code>.
		/// </param>
		/// <param name="similarity">The <code>Similarity</code> implementation to be used for score
		/// computations.
		/// </param>
		/// <param name="norms">The field norms of the document fields for the <code>Term</code>.
		/// </param>
		public /*internal*/ TermScorer(Weight weight, TermDocs td, Similarity similarity, byte[] norms):base(similarity)
		{
			this.weight = weight;
			this.termDocs = td;
			this.norms = norms;
			this.weightValue = weight.GetValue();
			
			for (int i = 0; i < SCORE_CACHE_SIZE; i++)
				scoreCache[i] = GetSimilarity().Tf(i) * weightValue;
		}
		
		/// <deprecated> use {@link #Score(Collector)} instead. 
		/// </deprecated>
        [Obsolete("use Score(Collector) instead. ")]
		public override void  Score(HitCollector hc)
		{
			Score(new HitCollectorWrapper(hc));
		}
		
		public override void  Score(Collector c)
		{
			Score(c, System.Int32.MaxValue, NextDoc());
		}
		
		/// <deprecated> use {@link #Score(Collector, int, int)} instead. 
		/// </deprecated>
        [Obsolete("use Score(Collector, int, int) instead.")]
		protected internal override bool Score(HitCollector c, int end)
		{
			return Score(new HitCollectorWrapper(c), end, doc);
		}
		
		// firstDocID is ignored since nextDoc() sets 'doc'
		public /*protected internal*/ override bool Score(Collector c, int end, int firstDocID)
		{
			c.SetScorer(this);
			while (doc < end)
			{
				// for docs in window
				c.Collect(doc); // collect score
				
				if (++pointer >= pointerMax)
				{
					pointerMax = termDocs.Read(docs, freqs); // refill buffers
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
			}
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
		
		/// <summary> Advances to the next document matching the query. <br/>
		/// The iterator over the matching documents is buffered using
		/// {@link TermDocs#Read(int[],int[])}.
		/// 
		/// </summary>
		/// <returns> true iff there is another document matching the query.
		/// </returns>
		/// <deprecated> use {@link #NextDoc()} instead.
		/// </deprecated>
        [Obsolete("use NextDoc() instead.")]
		public override bool Next()
		{
			return NextDoc() != NO_MORE_DOCS;
		}
		
		/// <summary> Advances to the next document matching the query. <br/>
		/// The iterator over the matching documents is buffered using
		/// {@link TermDocs#Read(int[],int[])}.
		/// 
		/// </summary>
		/// <returns> the document matching the query or -1 if there are no more documents.
		/// </returns>
		public override int NextDoc()
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
					return doc = NO_MORE_DOCS;
				}
			}
			doc = docs[pointer];
			return doc;
		}
		
		public override float Score()
		{
			System.Diagnostics.Debug.Assert(doc != - 1);
			int f = freqs[pointer];
			float raw = f < SCORE_CACHE_SIZE?scoreCache[f]:GetSimilarity().Tf(f) * weightValue; // cache miss
			
			return norms == null?raw:raw * SIM_NORM_DECODER[norms[doc] & 0xFF]; // normalize for field
		}
		
		/// <summary> Skips to the first match beyond the current whose document number is
		/// greater than or equal to a given target. <br/>
		/// The implementation uses {@link TermDocs#SkipTo(int)}.
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
		/// The implementation uses {@link TermDocs#SkipTo(int)}.
		/// 
		/// </summary>
		/// <param name="target">The target document number.
		/// </param>
		/// <returns> the matching document or -1 if none exist.
		/// </returns>
		public override int Advance(int target)
		{
			// first scan in cache
			for (pointer++; pointer < pointerMax; pointer++)
			{
				if (docs[pointer] >= target)
				{
					return doc = docs[pointer];
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
				doc = NO_MORE_DOCS;
			}
			return doc;
		}
		
		/// <summary>Returns an explanation of the score for a document.
		/// <br/>When this method is used, the {@link #Next()} method
		/// and the {@link #Score(HitCollector)} method should not be used.
		/// </summary>
		/// <param name="doc">The document number for the explanation.
		/// </param>
		public override Explanation Explain(int doc)
		{
			TermQuery query = (TermQuery) weight.GetQuery();
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
				if (termDocs.SkipTo(doc))
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
		
		/// <summary>Returns a string representation of this <code>TermScorer</code>. </summary>
		public override System.String ToString()
		{
			return "scorer(" + weight + ")";
		}
		static TermScorer()
		{
			SIM_NORM_DECODER = Similarity.GetNormDecoder();
		}
	}
}
