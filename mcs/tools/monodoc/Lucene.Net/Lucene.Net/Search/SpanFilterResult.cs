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

namespace Mono.Lucene.Net.Search
{
	
	
	/// <summary>  The results of a SpanQueryFilter.  Wraps the BitSet and the position information from the SpanQuery
	/// 
	/// <p/>
	/// NOTE: This API is still experimental and subject to change. 
	/// 
	/// 
	/// </summary>
	public class SpanFilterResult
	{
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		private System.Collections.BitArray bits;
		
		private DocIdSet docIdSet;
		private System.Collections.IList positions; //Spans spans;
		
		/// <summary> </summary>
		/// <param name="bits">The bits for the Filter
		/// </param>
		/// <param name="positions">A List of {@link Mono.Lucene.Net.Search.SpanFilterResult.PositionInfo} objects
		/// </param>
		/// <deprecated> Use {@link #SpanFilterResult(DocIdSet, List)} instead
		/// </deprecated>
        [Obsolete("Use SpanFilterResult(DocIdSet, List) instead")]
		public SpanFilterResult(System.Collections.BitArray bits, System.Collections.IList positions)
		{
			this.bits = bits;
			this.positions = positions;
		}
		
		/// <summary> </summary>
		/// <param name="docIdSet">The DocIdSet for the Filter
		/// </param>
		/// <param name="positions">A List of {@link Mono.Lucene.Net.Search.SpanFilterResult.PositionInfo} objects
		/// </param>
		public SpanFilterResult(DocIdSet docIdSet, System.Collections.IList positions)
		{
			this.docIdSet = docIdSet;
			this.positions = positions;
		}
		
		/// <summary> The first entry in the array corresponds to the first "on" bit.
		/// Entries are increasing by document order
		/// </summary>
		/// <returns> A List of PositionInfo objects
		/// </returns>
		public virtual System.Collections.IList GetPositions()
		{
			return positions;
		}
		
		/// <deprecated> Use {@link #GetDocIdSet()}
		/// </deprecated>
        [Obsolete("Use GetDocIdSet()")]
		public virtual System.Collections.BitArray GetBits()
		{
			return bits;
		}
		
		/// <summary>Returns the docIdSet </summary>
		public virtual DocIdSet GetDocIdSet()
		{
			return docIdSet;
		}
		
		public class PositionInfo
		{
			private int doc;
			private System.Collections.IList positions;
			
			
			public PositionInfo(int doc)
			{
				this.doc = doc;
				positions = new System.Collections.ArrayList();
			}
			
			public virtual void  AddPosition(int start, int end)
			{
				positions.Add(new StartEnd(start, end));
			}
			
			public virtual int GetDoc()
			{
				return doc;
			}
			
			/// <summary> </summary>
			/// <returns> A List of {@link Mono.Lucene.Net.Search.SpanFilterResult.StartEnd} objects
			/// </returns>
			public virtual System.Collections.IList GetPositions()
			{
				return positions;
			}
		}
		
		public class StartEnd
		{
			private int start;
			private int end;
			
			
			public StartEnd(int start, int end)
			{
				this.start = start;
				this.end = end;
			}
			
			/// <summary> </summary>
			/// <returns> The end position of this match
			/// </returns>
			public virtual int GetEnd()
			{
				return end;
			}
			
			/// <summary> The Start position</summary>
			/// <returns> The start position of this match
			/// </returns>
			public virtual int GetStart()
			{
				return start;
			}
		}
	}
}
