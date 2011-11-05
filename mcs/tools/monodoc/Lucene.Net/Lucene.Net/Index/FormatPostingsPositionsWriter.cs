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

using IndexInput = Mono.Lucene.Net.Store.IndexInput;
using IndexOutput = Mono.Lucene.Net.Store.IndexOutput;

namespace Mono.Lucene.Net.Index
{
	
	sealed class FormatPostingsPositionsWriter:FormatPostingsPositionsConsumer
	{
		
		internal FormatPostingsDocsWriter parent;
		internal IndexOutput out_Renamed;
		
		internal bool omitTermFreqAndPositions;
		internal bool storePayloads;
		internal int lastPayloadLength = - 1;
		
		internal FormatPostingsPositionsWriter(SegmentWriteState state, FormatPostingsDocsWriter parent)
		{
			this.parent = parent;
			omitTermFreqAndPositions = parent.omitTermFreqAndPositions;
			if (parent.parent.parent.fieldInfos.HasProx())
			{
				// At least one field does not omit TF, so create the
				// prox file
				System.String fileName = IndexFileNames.SegmentFileName(parent.parent.parent.segment, IndexFileNames.PROX_EXTENSION);
				SupportClass.CollectionsHelper.AddIfNotContains(state.flushedFiles, fileName);
				out_Renamed = parent.parent.parent.dir.CreateOutput(fileName);
				parent.skipListWriter.SetProxOutput(out_Renamed);
			}
			// Every field omits TF so we will write no prox file
			else
				out_Renamed = null;
		}
		
		internal int lastPosition;
		
		/// <summary>Add a new position &amp; payload </summary>
		internal override void  AddPosition(int position, byte[] payload, int payloadOffset, int payloadLength)
		{
			System.Diagnostics.Debug.Assert(!omitTermFreqAndPositions, "omitTermFreqAndPositions is true");
			System.Diagnostics.Debug.Assert(out_Renamed != null);
			
			int delta = position - lastPosition;
			lastPosition = position;
			
			if (storePayloads)
			{
				if (payloadLength != lastPayloadLength)
				{
					lastPayloadLength = payloadLength;
					out_Renamed.WriteVInt((delta << 1) | 1);
					out_Renamed.WriteVInt(payloadLength);
				}
				else
					out_Renamed.WriteVInt(delta << 1);
				if (payloadLength > 0)
					out_Renamed.WriteBytes(payload, payloadLength);
			}
			else
				out_Renamed.WriteVInt(delta);
		}
		
		internal void  SetField(FieldInfo fieldInfo)
		{
			omitTermFreqAndPositions = fieldInfo.omitTermFreqAndPositions;
			storePayloads = omitTermFreqAndPositions?false:fieldInfo.storePayloads;
		}
		
		/// <summary>Called when we are done adding positions &amp; payloads </summary>
		internal override void  Finish()
		{
			lastPosition = 0;
			lastPayloadLength = - 1;
		}
		
		internal void  Close()
		{
			if (out_Renamed != null)
				out_Renamed.Close();
		}
	}
}
