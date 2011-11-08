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

using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;

namespace Mono.Lucene.Net.Index
{
	
	
	/// <summary> Implements the skip list writer for the default posting list format
	/// that stores positions and payloads.
	/// 
	/// </summary>
	class DefaultSkipListWriter:MultiLevelSkipListWriter
	{
		private int[] lastSkipDoc;
		private int[] lastSkipPayloadLength;
		private long[] lastSkipFreqPointer;
		private long[] lastSkipProxPointer;
		
		private IndexOutput freqOutput;
		private IndexOutput proxOutput;
		
		private int curDoc;
		private bool curStorePayloads;
		private int curPayloadLength;
		private long curFreqPointer;
		private long curProxPointer;
		
		internal DefaultSkipListWriter(int skipInterval, int numberOfSkipLevels, int docCount, IndexOutput freqOutput, IndexOutput proxOutput):base(skipInterval, numberOfSkipLevels, docCount)
		{
			this.freqOutput = freqOutput;
			this.proxOutput = proxOutput;
			
			lastSkipDoc = new int[numberOfSkipLevels];
			lastSkipPayloadLength = new int[numberOfSkipLevels];
			lastSkipFreqPointer = new long[numberOfSkipLevels];
			lastSkipProxPointer = new long[numberOfSkipLevels];
		}
		
		internal virtual void  SetFreqOutput(IndexOutput freqOutput)
		{
			this.freqOutput = freqOutput;
		}
		
		internal virtual void  SetProxOutput(IndexOutput proxOutput)
		{
			this.proxOutput = proxOutput;
		}
		
		/// <summary> Sets the values for the current skip data. </summary>
		internal virtual void  SetSkipData(int doc, bool storePayloads, int payloadLength)
		{
			this.curDoc = doc;
			this.curStorePayloads = storePayloads;
			this.curPayloadLength = payloadLength;
			this.curFreqPointer = freqOutput.GetFilePointer();
			if (proxOutput != null)
				this.curProxPointer = proxOutput.GetFilePointer();
		}
		
		protected internal override void  ResetSkip()
		{
			base.ResetSkip();
			for (int i = 0; i < lastSkipDoc.Length; i++) lastSkipDoc[i] = 0;
			for (int i = 0; i < lastSkipPayloadLength.Length; i++) lastSkipPayloadLength[i] = -1; // we don't have to write the first length in the skip list
			for (int i = 0; i < lastSkipFreqPointer.Length; i++) lastSkipFreqPointer[i] = freqOutput.GetFilePointer();
			if (proxOutput != null)
				for (int i = 0; i < lastSkipProxPointer.Length; i++) lastSkipProxPointer[i] = proxOutput.GetFilePointer();
		}
		
		protected internal override void  WriteSkipData(int level, IndexOutput skipBuffer)
		{
			// To efficiently store payloads in the posting lists we do not store the length of
			// every payload. Instead we omit the length for a payload if the previous payload had
			// the same length.
			// However, in order to support skipping the payload length at every skip point must be known.
			// So we use the same length encoding that we use for the posting lists for the skip data as well:
			// Case 1: current field does not store payloads
			//           SkipDatum                 --> DocSkip, FreqSkip, ProxSkip
			//           DocSkip,FreqSkip,ProxSkip --> VInt
			//           DocSkip records the document number before every SkipInterval th  document in TermFreqs. 
			//           Document numbers are represented as differences from the previous value in the sequence.
			// Case 2: current field stores payloads
			//           SkipDatum                 --> DocSkip, PayloadLength?, FreqSkip,ProxSkip
			//           DocSkip,FreqSkip,ProxSkip --> VInt
			//           PayloadLength             --> VInt    
			//         In this case DocSkip/2 is the difference between
			//         the current and the previous value. If DocSkip
			//         is odd, then a PayloadLength encoded as VInt follows,
			//         if DocSkip is even, then it is assumed that the
			//         current payload length equals the length at the previous
			//         skip point
			if (curStorePayloads)
			{
				int delta = curDoc - lastSkipDoc[level];
				if (curPayloadLength == lastSkipPayloadLength[level])
				{
					// the current payload length equals the length at the previous skip point,
					// so we don't store the length again
					skipBuffer.WriteVInt(delta * 2);
				}
				else
				{
					// the payload length is different from the previous one. We shift the DocSkip, 
					// set the lowest bit and store the current payload length as VInt.
					skipBuffer.WriteVInt(delta * 2 + 1);
					skipBuffer.WriteVInt(curPayloadLength);
					lastSkipPayloadLength[level] = curPayloadLength;
				}
			}
			else
			{
				// current field does not store payloads
				skipBuffer.WriteVInt(curDoc - lastSkipDoc[level]);
			}
			skipBuffer.WriteVInt((int) (curFreqPointer - lastSkipFreqPointer[level]));
			skipBuffer.WriteVInt((int) (curProxPointer - lastSkipProxPointer[level]));
			
			lastSkipDoc[level] = curDoc;
			//System.out.println("write doc at level " + level + ": " + curDoc);
			
			lastSkipFreqPointer[level] = curFreqPointer;
			lastSkipProxPointer[level] = curProxPointer;
		}
	}
}
