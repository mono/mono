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
using ToStringUtils = Mono.Lucene.Net.Util.ToStringUtils;
using Explanation = Mono.Lucene.Net.Search.Explanation;
using Scorer = Mono.Lucene.Net.Search.Scorer;
using Searcher = Mono.Lucene.Net.Search.Searcher;
using Similarity = Mono.Lucene.Net.Search.Similarity;
using Weight = Mono.Lucene.Net.Search.Weight;
using NearSpansOrdered = Mono.Lucene.Net.Search.Spans.NearSpansOrdered;
using NearSpansUnordered = Mono.Lucene.Net.Search.Spans.NearSpansUnordered;
using SpanNearQuery = Mono.Lucene.Net.Search.Spans.SpanNearQuery;
using SpanQuery = Mono.Lucene.Net.Search.Spans.SpanQuery;
using SpanScorer = Mono.Lucene.Net.Search.Spans.SpanScorer;
using SpanWeight = Mono.Lucene.Net.Search.Spans.SpanWeight;

namespace Mono.Lucene.Net.Search.Payloads
{
	
	/// <summary> This class is very similar to
	/// {@link Mono.Lucene.Net.Search.Spans.SpanNearQuery} except that it factors
	/// in the value of the payloads located at each of the positions where the
	/// {@link Mono.Lucene.Net.Search.Spans.TermSpans} occurs.
	/// <p/>
	/// In order to take advantage of this, you must override
	/// {@link Mono.Lucene.Net.Search.Similarity#ScorePayload(String, byte[],int,int)}
	/// which returns 1 by default.
	/// <p/>
	/// Payload scores are aggregated using a pluggable {@link PayloadFunction}.
	/// 
	/// </summary>
	/// <seealso cref="Mono.Lucene.Net.Search.Similarity.ScorePayload(String, byte[], int,int)">
	/// </seealso>
	[Serializable]
	public class PayloadNearQuery:SpanNearQuery, System.ICloneable
	{
		protected internal System.String fieldName;
		protected internal PayloadFunction function;
		
		public PayloadNearQuery(SpanQuery[] clauses, int slop, bool inOrder):this(clauses, slop, inOrder, new AveragePayloadFunction())
		{
		}
		
		public PayloadNearQuery(SpanQuery[] clauses, int slop, bool inOrder, PayloadFunction function):base(clauses, slop, inOrder)
		{
			fieldName = clauses[0].GetField(); // all clauses must have same field
			this.function = function;
		}
		
		public override Weight CreateWeight(Searcher searcher)
		{
			return new PayloadNearSpanWeight(this, this, searcher);
		}
		
		public override System.Object Clone()
		{
			int sz = clauses.Count;
			SpanQuery[] newClauses = new SpanQuery[sz];
			
			for (int i = 0; i < sz; i++)
			{
				SpanQuery clause = (SpanQuery) clauses[i];
				newClauses[i] = (SpanQuery) clause.Clone();
			}
			PayloadNearQuery boostingNearQuery = new PayloadNearQuery(newClauses, slop, inOrder);
			boostingNearQuery.SetBoost(GetBoost());
			return boostingNearQuery;
		}
		
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("payloadNear([");
			System.Collections.IEnumerator i = clauses.GetEnumerator();
			while (i.MoveNext())
			{
				SpanQuery clause = (SpanQuery) i.Current;
				buffer.Append(clause.ToString(field));
				if (i.MoveNext())
				{
					buffer.Append(", ");
				}
			}
			buffer.Append("], ");
			buffer.Append(slop);
			buffer.Append(", ");
			buffer.Append(inOrder);
			buffer.Append(")");
			buffer.Append(ToStringUtils.Boost(GetBoost()));
			return buffer.ToString();
		}
		
		// @Override
		public override int GetHashCode()
		{
			int prime = 31;
			int result = base.GetHashCode();
			result = prime * result + ((fieldName == null)?0:fieldName.GetHashCode());
			result = prime * result + ((function == null)?0:function.GetHashCode());
			return result;
		}
		
		// @Override
		public  override bool Equals(System.Object obj)
		{
			if (this == obj)
				return true;
			if (!base.Equals(obj))
				return false;
			if (GetType() != obj.GetType())
				return false;
			PayloadNearQuery other = (PayloadNearQuery) obj;
			if (fieldName == null)
			{
				if (other.fieldName != null)
					return false;
			}
			else if (!fieldName.Equals(other.fieldName))
				return false;
			if (function == null)
			{
				if (other.function != null)
					return false;
			}
			else if (!function.Equals(other.function))
				return false;
			return true;
		}
		
		[Serializable]
		public class PayloadNearSpanWeight:SpanWeight
		{
			private void  InitBlock(PayloadNearQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private PayloadNearQuery enclosingInstance;
			public PayloadNearQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public PayloadNearSpanWeight(PayloadNearQuery enclosingInstance, SpanQuery query, Searcher searcher):base(query, searcher)
			{
				InitBlock(enclosingInstance);
			}
			
			public virtual Scorer Scorer(IndexReader reader)
			{
				return new PayloadNearSpanScorer(enclosingInstance, query.GetSpans(reader), this, similarity, reader.Norms(query.GetField()));
			}
			
			public override Scorer Scorer(IndexReader reader, bool scoreDocsInOrder, bool topScorer)
			{
				return new PayloadNearSpanScorer(enclosingInstance, query.GetSpans(reader), this, similarity, reader.Norms(query.GetField()));
			}
		}
		
		public class PayloadNearSpanScorer:SpanScorer
		{
			private void  InitBlock(PayloadNearQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
				similarity = GetSimilarity();
			}
			private PayloadNearQuery enclosingInstance;
			public PayloadNearQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			new internal Mono.Lucene.Net.Search.Spans.Spans spans;
			
			protected internal float payloadScore;
			private int payloadsSeen;
			internal Similarity similarity;
			
			protected internal PayloadNearSpanScorer(PayloadNearQuery enclosingInstance, Mono.Lucene.Net.Search.Spans.Spans spans, Weight weight, Similarity similarity, byte[] norms):base(spans, weight, similarity, norms)
			{
				InitBlock(enclosingInstance);
				this.spans = spans;
			}
			
			// Get the payloads associated with all underlying subspans
			public virtual void  GetPayloads(Mono.Lucene.Net.Search.Spans.Spans[] subSpans)
			{
				for (int i = 0; i < subSpans.Length; i++)
				{
					if (subSpans[i] is NearSpansOrdered)
					{
						if (((NearSpansOrdered) subSpans[i]).IsPayloadAvailable())
						{
							ProcessPayloads(((NearSpansOrdered) subSpans[i]).GetPayload(), subSpans[i].Start(), subSpans[i].End());
						}
						GetPayloads(((NearSpansOrdered) subSpans[i]).GetSubSpans());
					}
					else if (subSpans[i] is NearSpansUnordered)
					{
						if (((NearSpansUnordered) subSpans[i]).IsPayloadAvailable())
						{
							ProcessPayloads(((NearSpansUnordered) subSpans[i]).GetPayload(), subSpans[i].Start(), subSpans[i].End());
						}
						GetPayloads(((NearSpansUnordered) subSpans[i]).GetSubSpans());
					}
				}
			}
			
			/// <summary> By default, uses the {@link PayloadFunction} to score the payloads, but
			/// can be overridden to do other things.
			/// 
			/// </summary>
			/// <param name="payLoads">The payloads
			/// </param>
			/// <param name="start">The start position of the span being scored
			/// </param>
			/// <param name="end">The end position of the span being scored
			/// 
			/// </param>
			/// <seealso cref="Spans">
			/// </seealso>
			protected internal virtual void  ProcessPayloads(System.Collections.Generic.ICollection<byte[]> payLoads, int start, int end)
			{
                foreach (byte[] thePayload in payLoads)
                {
                    payloadScore = Enclosing_Instance.function.CurrentScore(doc, Enclosing_Instance.fieldName, start, end, payloadsSeen, payloadScore, similarity.ScorePayload(doc, Enclosing_Instance.fieldName, spans.Start(), spans.End(), thePayload, 0, thePayload.Length));
                    ++payloadsSeen;
                }
			}
			
			//
			public /*protected internal*/ override bool SetFreqCurrentDoc()
			{
				if (!more)
				{
					return false;
				}
				Mono.Lucene.Net.Search.Spans.Spans[] spansArr = new Mono.Lucene.Net.Search.Spans.Spans[1];
				spansArr[0] = spans;
				payloadScore = 0;
				payloadsSeen = 0;
				GetPayloads(spansArr);
				return base.SetFreqCurrentDoc();
			}
			
			public override float Score()
			{
				
				return base.Score() * Enclosing_Instance.function.DocScore(doc, Enclosing_Instance.fieldName, payloadsSeen, payloadScore);
			}
			
			public override Explanation Explain(int doc)
			{
				Explanation result = new Explanation();
				Explanation nonPayloadExpl = base.Explain(doc);
				result.AddDetail(nonPayloadExpl);
				Explanation payloadBoost = new Explanation();
				result.AddDetail(payloadBoost);
				float avgPayloadScore = (payloadsSeen > 0?(payloadScore / payloadsSeen):1);
				payloadBoost.SetValue(avgPayloadScore);
				payloadBoost.SetDescription("scorePayload(...)");
				result.SetValue(nonPayloadExpl.GetValue() * avgPayloadScore);
				result.SetDescription("bnq, product of:");
				return result;
			}
		}
	}
}
