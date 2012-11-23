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
using Query = Mono.Lucene.Net.Search.Query;
using Searcher = Mono.Lucene.Net.Search.Searcher;
using Similarity = Mono.Lucene.Net.Search.Similarity;
using Weight = Mono.Lucene.Net.Search.Weight;

namespace Mono.Lucene.Net.Search.Spans
{
	
	/// <summary> <p/>Wrapper to allow {@link SpanQuery} objects participate in composite 
	/// single-field SpanQueries by 'lying' about their search field. That is, 
	/// the masked SpanQuery will function as normal, 
	/// but {@link SpanQuery#GetField()} simply hands back the value supplied 
	/// in this class's constructor.<p/>
	/// 
	/// <p/>This can be used to support Queries like {@link SpanNearQuery} or 
	/// {@link SpanOrQuery} across different fields, which is not ordinarily 
	/// permitted.<p/>
	/// 
	/// <p/>This can be useful for denormalized relational data: for example, when 
	/// indexing a document with conceptually many 'children': <p/>
	/// 
	/// <pre>
	/// teacherid: 1
	/// studentfirstname: james
	/// studentsurname: jones
	/// 
	/// teacherid: 2
	/// studenfirstname: james
	/// studentsurname: smith
	/// studentfirstname: sally
	/// studentsurname: jones
	/// </pre>
	/// 
	/// <p/>a SpanNearQuery with a slop of 0 can be applied across two 
	/// {@link SpanTermQuery} objects as follows:
	/// <pre>
	/// SpanQuery q1  = new SpanTermQuery(new Term("studentfirstname", "james"));
	/// SpanQuery q2  = new SpanTermQuery(new Term("studentsurname", "jones"));
	/// SpanQuery q2m new FieldMaskingSpanQuery(q2, "studentfirstname");
	/// Query q = new SpanNearQuery(new SpanQuery[]{q1, q2m}, -1, false);
	/// </pre>
	/// to search for 'studentfirstname:james studentsurname:jones' and find 
	/// teacherid 1 without matching teacherid 2 (which has a 'james' in position 0 
	/// and 'jones' in position 1). <p/>
	/// 
	/// <p/>Note: as {@link #GetField()} returns the masked field, scoring will be 
	/// done using the norms of the field name supplied. This may lead to unexpected
	/// scoring behaviour.<p/>
	/// </summary>
	[Serializable]
	public class FieldMaskingSpanQuery:SpanQuery
	{
		private SpanQuery maskedQuery;
		private System.String field;
		
		public FieldMaskingSpanQuery(SpanQuery maskedQuery, System.String maskedField)
		{
			this.maskedQuery = maskedQuery;
			this.field = maskedField;
		}
		
		public override System.String GetField()
		{
			return field;
		}
		
		public virtual SpanQuery GetMaskedQuery()
		{
			return maskedQuery;
		}
		
		// :NOTE: getBoost and setBoost are not proxied to the maskedQuery
		// ...this is done to be more consistent with thigns like SpanFirstQuery
		
		public override Spans GetSpans(IndexReader reader)
		{
			return maskedQuery.GetSpans(reader);
		}
		
		/// <deprecated> use {@link #ExtractTerms(Set)} instead. 
		/// </deprecated>
        [Obsolete("use ExtractTerms(Hashtable) instead.")]
		public override System.Collections.ICollection GetTerms()
		{
			return maskedQuery.GetTerms();
		}
		
		public override void  ExtractTerms(System.Collections.Hashtable terms)
		{
			maskedQuery.ExtractTerms(terms);
		}
		
		public override Weight CreateWeight(Searcher searcher)
		{
			return maskedQuery.CreateWeight(searcher);
		}
		
		public override Similarity GetSimilarity(Searcher searcher)
		{
			return maskedQuery.GetSimilarity(searcher);
		}
		
		public override Query Rewrite(IndexReader reader)
		{
			FieldMaskingSpanQuery clone = null;
			
			SpanQuery rewritten = (SpanQuery) maskedQuery.Rewrite(reader);
			if (rewritten != maskedQuery)
			{
				clone = (FieldMaskingSpanQuery) this.Clone();
				clone.maskedQuery = rewritten;
			}
			
			if (clone != null)
			{
				return clone;
			}
			else
			{
				return this;
			}
		}
		
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("mask(");
			buffer.Append(maskedQuery.ToString(field));
			buffer.Append(")");
			buffer.Append(ToStringUtils.Boost(GetBoost()));
			buffer.Append(" as ");
			buffer.Append(this.field);
			return buffer.ToString();
		}
		
		public  override bool Equals(System.Object o)
		{
			if (!(o is FieldMaskingSpanQuery))
				return false;
			FieldMaskingSpanQuery other = (FieldMaskingSpanQuery) o;
			return (this.GetField().Equals(other.GetField()) && (this.GetBoost() == other.GetBoost()) && this.GetMaskedQuery().Equals(other.GetMaskedQuery()));
		}
		
		public override int GetHashCode()
		{
			return GetMaskedQuery().GetHashCode() ^ GetField().GetHashCode() ^ System.Convert.ToInt32(GetBoost());
		}
	}
}
