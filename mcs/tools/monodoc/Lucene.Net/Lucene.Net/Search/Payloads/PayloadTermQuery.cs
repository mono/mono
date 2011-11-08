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
using Term = Mono.Lucene.Net.Index.Term;
using TermPositions = Mono.Lucene.Net.Index.TermPositions;
using ComplexExplanation = Mono.Lucene.Net.Search.ComplexExplanation;
using Explanation = Mono.Lucene.Net.Search.Explanation;
using Scorer = Mono.Lucene.Net.Search.Scorer;
using Searcher = Mono.Lucene.Net.Search.Searcher;
using Similarity = Mono.Lucene.Net.Search.Similarity;
using Weight = Mono.Lucene.Net.Search.Weight;
using SpanScorer = Mono.Lucene.Net.Search.Spans.SpanScorer;
using SpanTermQuery = Mono.Lucene.Net.Search.Spans.SpanTermQuery;
using SpanWeight = Mono.Lucene.Net.Search.Spans.SpanWeight;
using TermSpans = Mono.Lucene.Net.Search.Spans.TermSpans;

namespace Mono.Lucene.Net.Search.Payloads
{
	
	/// <summary> This class is very similar to
	/// {@link Mono.Lucene.Net.Search.Spans.SpanTermQuery} except that it factors
	/// in the value of the payload located at each of the positions where the
	/// {@link Mono.Lucene.Net.Index.Term} occurs.
	/// <p/>
	/// In order to take advantage of this, you must override
	/// {@link Mono.Lucene.Net.Search.Similarity#ScorePayload(String, byte[],int,int)}
	/// which returns 1 by default.
	/// <p/>
	/// Payload scores are aggregated using a pluggable {@link PayloadFunction}.
	/// 
	/// </summary>
	[Serializable]
	public class PayloadTermQuery:SpanTermQuery
	{
		protected internal PayloadFunction function;
		private bool includeSpanScore;
		
		public PayloadTermQuery(Term term, PayloadFunction function):this(term, function, true)
		{
		}
		
		public PayloadTermQuery(Term term, PayloadFunction function, bool includeSpanScore):base(term)
		{
			this.function = function;
			this.includeSpanScore = includeSpanScore;
		}
		
		public override Weight CreateWeight(Searcher searcher)
		{
			return new PayloadTermWeight(this, this, searcher);
		}
		
		[Serializable]
		protected internal class PayloadTermWeight:SpanWeight
		{
			private void  InitBlock(PayloadTermQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private PayloadTermQuery enclosingInstance;
			public PayloadTermQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			public PayloadTermWeight(PayloadTermQuery enclosingInstance, PayloadTermQuery query, Searcher searcher):base(query, searcher)
			{
				InitBlock(enclosingInstance);
			}
			
			public override Scorer Scorer(IndexReader reader, bool scoreDocsInOrder, bool topScorer)
			{
				return new PayloadTermSpanScorer(this, (TermSpans) query.GetSpans(reader), this, similarity, reader.Norms(query.GetField()));
			}
			
			protected internal class PayloadTermSpanScorer:SpanScorer
			{
				private void  InitBlock(PayloadTermWeight enclosingInstance)
				{
					this.enclosingInstance = enclosingInstance;
				}
				private PayloadTermWeight enclosingInstance;
				public PayloadTermWeight Enclosing_Instance
				{
					get
					{
						return enclosingInstance;
					}
					
				}
				// TODO: is this the best way to allocate this?
				protected internal byte[] payload = new byte[256];
				protected internal TermPositions positions;
				protected internal float payloadScore;
				protected internal int payloadsSeen;
				
				public PayloadTermSpanScorer(PayloadTermWeight enclosingInstance, TermSpans spans, Weight weight, Similarity similarity, byte[] norms):base(spans, weight, similarity, norms)
				{
					InitBlock(enclosingInstance);
					positions = spans.GetPositions();
				}
				
				public /*protected internal*/ override bool SetFreqCurrentDoc()
				{
					if (!more)
					{
						return false;
					}
					doc = spans.Doc();
					freq = 0.0f;
					payloadScore = 0;
					payloadsSeen = 0;
					Similarity similarity1 = GetSimilarity();
					while (more && doc == spans.Doc())
					{
						int matchLength = spans.End() - spans.Start();
						
						freq += similarity1.SloppyFreq(matchLength);
						ProcessPayload(similarity1);
						
						more = spans.Next(); // this moves positions to the next match in this
						// document
					}
					return more || (freq != 0);
				}
				
				protected internal virtual void  ProcessPayload(Similarity similarity)
				{
					if (positions.IsPayloadAvailable())
					{
						payload = positions.GetPayload(payload, 0);
						payloadScore = Enclosing_Instance.Enclosing_Instance.function.CurrentScore(doc, Enclosing_Instance.Enclosing_Instance.term.Field(), spans.Start(), spans.End(), payloadsSeen, payloadScore, similarity.ScorePayload(doc, Enclosing_Instance.Enclosing_Instance.term.Field(), spans.Start(), spans.End(), payload, 0, positions.GetPayloadLength()));
						payloadsSeen++;
					}
					else
					{
						// zero out the payload?
					}
				}
				
				/// <summary> </summary>
				/// <returns> {@link #GetSpanScore()} * {@link #GetPayloadScore()}
				/// </returns>
				/// <throws>  IOException </throws>
				public override float Score()
				{
					
					return Enclosing_Instance.Enclosing_Instance.includeSpanScore?GetSpanScore() * GetPayloadScore():GetPayloadScore();
				}
				
				/// <summary> Returns the SpanScorer score only.
				/// <p/>
				/// Should not be overriden without good cause!
				/// 
				/// </summary>
				/// <returns> the score for just the Span part w/o the payload
				/// </returns>
				/// <throws>  IOException </throws>
				/// <summary> 
				/// </summary>
				/// <seealso cref="Score()">
				/// </seealso>
				protected internal virtual float GetSpanScore()
				{
					return base.Score();
				}
				
				/// <summary> The score for the payload
				/// 
				/// </summary>
				/// <returns> The score, as calculated by
				/// {@link PayloadFunction#DocScore(int, String, int, float)}
				/// </returns>
				protected internal virtual float GetPayloadScore()
				{
					return Enclosing_Instance.Enclosing_Instance.function.DocScore(doc, Enclosing_Instance.Enclosing_Instance.term.Field(), payloadsSeen, payloadScore);
				}
				
				public override Explanation Explain(int doc)
				{
					ComplexExplanation result = new ComplexExplanation();
					Explanation nonPayloadExpl = base.Explain(doc);
					result.AddDetail(nonPayloadExpl);
					// QUESTION: Is there a way to avoid this skipTo call? We need to know
					// whether to load the payload or not
					Explanation payloadBoost = new Explanation();
					result.AddDetail(payloadBoost);
					
					float payloadScore = GetPayloadScore();
					payloadBoost.SetValue(payloadScore);
					// GSI: I suppose we could toString the payload, but I don't think that
					// would be a good idea
					payloadBoost.SetDescription("scorePayload(...)");
					result.SetValue(nonPayloadExpl.GetValue() * payloadScore);
					result.SetDescription("btq, product of:");
					result.SetMatch(nonPayloadExpl.GetValue() == 0?false:true); // LUCENE-1303
					return result;
				}
			}
		}
		
		public override int GetHashCode()
		{
			int prime = 31;
			int result = base.GetHashCode();
			result = prime * result + ((function == null)?0:function.GetHashCode());
			result = prime * result + (includeSpanScore?1231:1237);
			return result;
		}
		
		public  override bool Equals(System.Object obj)
		{
			if (this == obj)
				return true;
			if (!base.Equals(obj))
				return false;
			if (GetType() != obj.GetType())
				return false;
			PayloadTermQuery other = (PayloadTermQuery) obj;
			if (function == null)
			{
				if (other.function != null)
					return false;
			}
			else if (!function.Equals(other.function))
				return false;
			if (includeSpanScore != other.includeSpanScore)
				return false;
			return true;
		}
	}
}
