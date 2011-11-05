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

namespace Mono.Lucene.Net.Index
{
	
	// TODO FI: some of this is "generic" to TermsHash* so we
	// should factor it out so other consumers don't have to
	// duplicate this code
	
	/// <summary>Used by DocumentsWriter to merge the postings from
	/// multiple ThreadStates when creating a segment 
	/// </summary>
	sealed class FreqProxFieldMergeState
	{
		
		internal FreqProxTermsWriterPerField field;
		internal int numPostings;
		internal CharBlockPool charPool;
		internal RawPostingList[] postings;
		
		private FreqProxTermsWriter.PostingList p;
		internal char[] text;
		internal int textOffset;
		
		private int postingUpto = - 1;
		
		internal ByteSliceReader freq = new ByteSliceReader();
		internal ByteSliceReader prox = new ByteSliceReader();
		
		internal int docID;
		internal int termFreq;
		
		public FreqProxFieldMergeState(FreqProxTermsWriterPerField field)
		{
			this.field = field;
			this.charPool = field.perThread.termsHashPerThread.charPool;
			this.numPostings = field.termsHashPerField.numPostings;
			this.postings = field.termsHashPerField.SortPostings();
		}
		
		internal bool NextTerm()
		{
			postingUpto++;
			if (postingUpto == numPostings)
				return false;
			
			p = (FreqProxTermsWriter.PostingList) postings[postingUpto];
			docID = 0;
			
			text = charPool.buffers[p.textStart >> DocumentsWriter.CHAR_BLOCK_SHIFT];
			textOffset = p.textStart & DocumentsWriter.CHAR_BLOCK_MASK;
			
			field.termsHashPerField.InitReader(freq, p, 0);
			if (!field.fieldInfo.omitTermFreqAndPositions)
				field.termsHashPerField.InitReader(prox, p, 1);
			
			// Should always be true
			bool result = NextDoc();
			System.Diagnostics.Debug.Assert(result);
			
			return true;
		}
		
		public bool NextDoc()
		{
			if (freq.Eof())
			{
				if (p.lastDocCode != - 1)
				{
					// Return last doc
					docID = p.lastDocID;
					if (!field.omitTermFreqAndPositions)
						termFreq = p.docFreq;
					p.lastDocCode = - 1;
					return true;
				}
				// EOF
				else
					return false;
			}
			
			int code = freq.ReadVInt();
			if (field.omitTermFreqAndPositions)
				docID += code;
			else
			{
				docID += SupportClass.Number.URShift(code, 1);
				if ((code & 1) != 0)
					termFreq = 1;
				else
					termFreq = freq.ReadVInt();
			}
			
			System.Diagnostics.Debug.Assert(docID != p.lastDocID);
			
			return true;
		}
	}
}
