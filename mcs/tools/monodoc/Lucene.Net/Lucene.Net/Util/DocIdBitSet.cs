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

using DocIdSet = Mono.Lucene.Net.Search.DocIdSet;
using DocIdSetIterator = Mono.Lucene.Net.Search.DocIdSetIterator;

namespace Mono.Lucene.Net.Util
{
	
	
	/// <summary>Simple DocIdSet and DocIdSetIterator backed by a BitSet </summary>
	public class DocIdBitSet:DocIdSet
	{
		private System.Collections.BitArray bitSet;
		
		public DocIdBitSet(System.Collections.BitArray bitSet)
		{
			this.bitSet = bitSet;
		}
		
		public override DocIdSetIterator Iterator()
		{
			return new DocIdBitSetIterator(bitSet);
		}

		/// <summary>This DocIdSet implementation is cacheable.</summary>
		public override bool IsCacheable()
		{
			return true;
		}
		
		/// <summary> Returns the underlying BitSet. </summary>
		public virtual System.Collections.BitArray GetBitSet()
		{
			return this.bitSet;
		}
		
		private class DocIdBitSetIterator:DocIdSetIterator
		{
			private int docId;
			private System.Collections.BitArray bitSet;
			
			internal DocIdBitSetIterator(System.Collections.BitArray bitSet)
			{
				this.bitSet = bitSet;
				this.docId = - 1;
			}
			
			/// <deprecated> use {@link #DocID()} instead. 
			/// </deprecated>
            [Obsolete("use DocID() instead.")]
			public override int Doc()
			{
				System.Diagnostics.Debug.Assert(docId != - 1);
				return docId;
			}
			
			public override int DocID()
			{
				return docId;
			}
			
			/// <deprecated> use {@link #NextDoc()} instead. 
			/// </deprecated>
            [Obsolete("use NextDoc() instead.")]
			public override bool Next()
			{
				// (docId + 1) on next line requires -1 initial value for docNr:
				return NextDoc() != NO_MORE_DOCS;
			}
			
			public override int NextDoc()
			{
				// (docId + 1) on next line requires -1 initial value for docNr:
				int d = SupportClass.BitSetSupport.NextSetBit(bitSet, docId + 1);
				// -1 returned by BitSet.nextSetBit() when exhausted
				docId = d == - 1?NO_MORE_DOCS:d;
				return docId;
			}
			
			/// <deprecated> use {@link #Advance(int)} instead. 
			/// </deprecated>
            [Obsolete("use Advance(int) instead.")]
			public override bool SkipTo(int skipDocNr)
			{
				return Advance(skipDocNr) != NO_MORE_DOCS;
			}
			
			public override int Advance(int target)
			{
				int d = SupportClass.BitSetSupport.NextSetBit(bitSet, target);
				// -1 returned by BitSet.nextSetBit() when exhausted
				docId = d == - 1?NO_MORE_DOCS:d;
				return docId;
			}
		}
	}
}
