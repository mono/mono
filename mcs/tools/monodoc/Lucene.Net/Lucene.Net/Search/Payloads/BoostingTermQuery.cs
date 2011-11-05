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
using Scorer = Mono.Lucene.Net.Search.Scorer;
using Searcher = Mono.Lucene.Net.Search.Searcher;
using Weight = Mono.Lucene.Net.Search.Weight;
using TermSpans = Mono.Lucene.Net.Search.Spans.TermSpans;

namespace Mono.Lucene.Net.Search.Payloads
{
	
	/// <summary> The BoostingTermQuery is very similar to the {@link Mono.Lucene.Net.Search.Spans.SpanTermQuery} except
	/// that it factors in the value of the payload located at each of the positions where the
	/// {@link Mono.Lucene.Net.Index.Term} occurs.
	/// <p/>
	/// In order to take advantage of this, you must override {@link Mono.Lucene.Net.Search.Similarity#ScorePayload(String, byte[],int,int)}
	/// which returns 1 by default.
	/// <p/>
	/// Payload scores are averaged across term occurrences in the document.  
	/// 
	/// </summary>
	/// <seealso cref="Mono.Lucene.Net.Search.Similarity.ScorePayload(String, byte[], int, int)">
	/// 
	/// </seealso>
	/// <deprecated> See {@link Mono.Lucene.Net.Search.Payloads.PayloadTermQuery}
	/// </deprecated>
    [Obsolete("See Mono.Lucene.Net.Search.Payloads.PayloadTermQuery")]
	[Serializable]
	public class BoostingTermQuery:PayloadTermQuery
	{
		
		public BoostingTermQuery(Term term):this(term, true)
		{
		}
		
		public BoostingTermQuery(Term term, bool includeSpanScore):base(term, new AveragePayloadFunction(), includeSpanScore)
		{
		}
		
		public override Weight CreateWeight(Searcher searcher)
		{
			return new BoostingTermWeight(this, this, searcher);
		}
		
		[Serializable]
		protected internal class BoostingTermWeight:PayloadTermWeight
		{
			private void  InitBlock(BoostingTermQuery enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private BoostingTermQuery enclosingInstance;
			public new BoostingTermQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			public BoostingTermWeight(BoostingTermQuery enclosingInstance, BoostingTermQuery query, Searcher searcher):base(enclosingInstance, query, searcher)
			{
				InitBlock(enclosingInstance);
			}
			
			public override Scorer Scorer(IndexReader reader, bool scoreDocsInOrder, bool topScorer)
			{
				return new PayloadTermSpanScorer(this, (TermSpans) query.GetSpans(reader), this, similarity, reader.Norms(query.GetField()));
			}
		}
		
		
		public  override bool Equals(System.Object o)
		{
			if (!(o is BoostingTermQuery))
				return false;
			BoostingTermQuery other = (BoostingTermQuery) o;
			return (this.GetBoost() == other.GetBoost()) && this.term.Equals(other.term);
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
