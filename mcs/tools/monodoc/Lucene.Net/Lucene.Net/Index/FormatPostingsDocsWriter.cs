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


/// <summary>Consumes doc & freq, writing them using the current
/// index file format 
/// </summary>

using System;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;
using UnicodeUtil = Mono.Lucene.Net.Util.UnicodeUtil;

namespace Mono.Lucene.Net.Index
{
	
	sealed class FormatPostingsDocsWriter:FormatPostingsDocsConsumer
	{
		
		internal IndexOutput out_Renamed;
		internal FormatPostingsTermsWriter parent;
		internal FormatPostingsPositionsWriter posWriter;
		internal DefaultSkipListWriter skipListWriter;
		internal int skipInterval;
		internal int totalNumDocs;
		
		internal bool omitTermFreqAndPositions;
		internal bool storePayloads;
		internal long freqStart;
		internal FieldInfo fieldInfo;
		
		internal FormatPostingsDocsWriter(SegmentWriteState state, FormatPostingsTermsWriter parent):base()
		{
			this.parent = parent;
			System.String fileName = IndexFileNames.SegmentFileName(parent.parent.segment, IndexFileNames.FREQ_EXTENSION);
			SupportClass.CollectionsHelper.AddIfNotContains(state.flushedFiles, fileName);
			out_Renamed = parent.parent.dir.CreateOutput(fileName);
			totalNumDocs = parent.parent.totalNumDocs;
			
			// TODO: abstraction violation
			skipInterval = parent.parent.termsOut.skipInterval;
			skipListWriter = parent.parent.skipListWriter;
			skipListWriter.SetFreqOutput(out_Renamed);
			
			posWriter = new FormatPostingsPositionsWriter(state, this);
		}
		
		internal void  SetField(FieldInfo fieldInfo)
		{
			this.fieldInfo = fieldInfo;
			omitTermFreqAndPositions = fieldInfo.omitTermFreqAndPositions;
			storePayloads = fieldInfo.storePayloads;
			posWriter.SetField(fieldInfo);
		}
		
		internal int lastDocID;
		internal int df;
		
		/// <summary>Adds a new doc in this term.  If this returns null
		/// then we just skip consuming positions/payloads. 
		/// </summary>
		internal override FormatPostingsPositionsConsumer AddDoc(int docID, int termDocFreq)
		{
			
			int delta = docID - lastDocID;
			
			if (docID < 0 || (df > 0 && delta <= 0))
				throw new CorruptIndexException("docs out of order (" + docID + " <= " + lastDocID + " )");
			
			if ((++df % skipInterval) == 0)
			{
				// TODO: abstraction violation
				skipListWriter.SetSkipData(lastDocID, storePayloads, posWriter.lastPayloadLength);
				skipListWriter.BufferSkip(df);
			}
			
			System.Diagnostics.Debug.Assert(docID < totalNumDocs, "docID=" + docID + " totalNumDocs=" + totalNumDocs);
			
			lastDocID = docID;
			if (omitTermFreqAndPositions)
				out_Renamed.WriteVInt(delta);
			else if (1 == termDocFreq)
				out_Renamed.WriteVInt((delta << 1) | 1);
			else
			{
				out_Renamed.WriteVInt(delta << 1);
				out_Renamed.WriteVInt(termDocFreq);
			}
			
			return posWriter;
		}
		
		private TermInfo termInfo = new TermInfo(); // minimize consing
		internal UnicodeUtil.UTF8Result utf8 = new UnicodeUtil.UTF8Result();
		
		/// <summary>Called when we are done adding docs to this term </summary>
		internal override void  Finish()
		{
			long skipPointer = skipListWriter.WriteSkip(out_Renamed);
			
			// TODO: this is abstraction violation -- we should not
			// peek up into parents terms encoding format
			termInfo.Set(df, parent.freqStart, parent.proxStart, (int) (skipPointer - parent.freqStart));
			
			// TODO: we could do this incrementally
			UnicodeUtil.UTF16toUTF8(parent.currentTerm, parent.currentTermStart, utf8);
			
			if (df > 0)
			{
				parent.termsOut.Add(fieldInfo.number, utf8.result, utf8.length, termInfo);
			}
			
			lastDocID = 0;
			df = 0;
		}
		
		internal void  Close()
		{
			out_Renamed.Close();
			posWriter.Close();
		}
	}
}
