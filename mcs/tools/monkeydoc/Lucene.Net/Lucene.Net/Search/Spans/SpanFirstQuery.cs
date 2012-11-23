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

namespace Mono.Lucene.Net.Search.Spans
{
	
	/// <summary>Matches spans near the beginning of a field. </summary>
	[Serializable]
	public class SpanFirstQuery:SpanQuery, System.ICloneable
	{
		private class AnonymousClassSpans : Spans
		{
			public AnonymousClassSpans(Mono.Lucene.Net.Index.IndexReader reader, SpanFirstQuery enclosingInstance)
			{
				InitBlock(reader, enclosingInstance);
			}
			private void  InitBlock(Mono.Lucene.Net.Index.IndexReader reader, SpanFirstQuery enclosingInstance)
			{
				this.reader = reader;
				this.enclosingInstance = enclosingInstance;
				spans = Enclosing_Instance.match.GetSpans(reader);
			}
			private Mono.Lucene.Net.Index.IndexReader reader;
			private SpanFirstQuery enclosingInstance;
			public SpanFirstQuery Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private Spans spans;
			
			public override bool Next()
			{
				while (spans.Next())
				{
					// scan to next match
					if (End() <= Enclosing_Instance.end)
						return true;
				}
				return false;
			}
			
			public override bool SkipTo(int target)
			{
				if (!spans.SkipTo(target))
					return false;
				
				return spans.End() <= Enclosing_Instance.end || Next();
			}
			
			public override int Doc()
			{
				return spans.Doc();
			}
			public override int Start()
			{
				return spans.Start();
			}
			public override int End()
			{
				return spans.End();
			}
			
			// TODO: Remove warning after API has been finalized
			public override System.Collections.Generic.ICollection<byte[]> GetPayload()
			{
				System.Collections.Generic.ICollection<byte[]> result = null;
				if (spans.IsPayloadAvailable())
				{
					result = spans.GetPayload();
				}
				return result; //TODO: any way to avoid the new construction?
			}
			
			// TODO: Remove warning after API has been finalized
			public override bool IsPayloadAvailable()
			{
				return spans.IsPayloadAvailable();
			}
			
			public override System.String ToString()
			{
				return "spans(" + Enclosing_Instance.ToString() + ")";
			}
		}
		private SpanQuery match;
		private int end;
		
		/// <summary>Construct a SpanFirstQuery matching spans in <code>match</code> whose end
		/// position is less than or equal to <code>end</code>. 
		/// </summary>
		public SpanFirstQuery(SpanQuery match, int end)
		{
			this.match = match;
			this.end = end;
		}
		
		/// <summary>Return the SpanQuery whose matches are filtered. </summary>
		public virtual SpanQuery GetMatch()
		{
			return match;
		}
		
		/// <summary>Return the maximum end position permitted in a match. </summary>
		public virtual int GetEnd()
		{
			return end;
		}
		
		public override System.String GetField()
		{
			return match.GetField();
		}
		
		/// <summary>Returns a collection of all terms matched by this query.</summary>
		/// <deprecated> use extractTerms instead
		/// </deprecated>
		/// <seealso cref="ExtractTerms(Set)">
		/// </seealso>
        [Obsolete("use ExtractTerms instead")]
		public override System.Collections.ICollection GetTerms()
		{
			return match.GetTerms();
		}
		
		public override System.String ToString(System.String field)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("spanFirst(");
			buffer.Append(match.ToString(field));
			buffer.Append(", ");
			buffer.Append(end);
			buffer.Append(")");
			buffer.Append(ToStringUtils.Boost(GetBoost()));
			return buffer.ToString();
		}
		
		public override System.Object Clone()
		{
			SpanFirstQuery spanFirstQuery = new SpanFirstQuery((SpanQuery) match.Clone(), end);
			spanFirstQuery.SetBoost(GetBoost());
			return spanFirstQuery;
		}
		
		public override void  ExtractTerms(System.Collections.Hashtable terms)
		{
			match.ExtractTerms(terms);
		}
		
		public override Spans GetSpans(IndexReader reader)
		{
			return new AnonymousClassSpans(reader, this);
		}
		
		public override Query Rewrite(IndexReader reader)
		{
			SpanFirstQuery clone = null;
			
			SpanQuery rewritten = (SpanQuery) match.Rewrite(reader);
			if (rewritten != match)
			{
				clone = (SpanFirstQuery) this.Clone();
				clone.match = rewritten;
			}
			
			if (clone != null)
			{
				return clone; // some clauses rewrote
			}
			else
			{
				return this; // no clauses rewrote
			}
		}
		
		public  override bool Equals(System.Object o)
		{
			if (this == o)
				return true;
			if (!(o is SpanFirstQuery))
				return false;
			
			SpanFirstQuery other = (SpanFirstQuery) o;
			return this.end == other.end && this.match.Equals(other.match) && this.GetBoost() == other.GetBoost();
		}
		
		public override int GetHashCode()
		{
			int h = match.GetHashCode();
			h ^= ((h << 8) | (SupportClass.Number.URShift(h, 25))); // reversible
			h ^= System.Convert.ToInt32(GetBoost()) ^ end;
			return h;
		}
	}
}
