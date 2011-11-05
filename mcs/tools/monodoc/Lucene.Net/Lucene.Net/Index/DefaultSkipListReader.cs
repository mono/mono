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

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> Implements the skip list reader for the default posting list format
	/// that stores positions and payloads.
	/// 
	/// </summary>
	class DefaultSkipListReader:MultiLevelSkipListReader
	{
		private bool currentFieldStoresPayloads;
		private long[] freqPointer;
		private long[] proxPointer;
		private int[] payloadLength;
		
		private long lastFreqPointer;
		private long lastProxPointer;
		private int lastPayloadLength;
		
		
		internal DefaultSkipListReader(IndexInput skipStream, int maxSkipLevels, int skipInterval):base(skipStream, maxSkipLevels, skipInterval)
		{
			freqPointer = new long[maxSkipLevels];
			proxPointer = new long[maxSkipLevels];
			payloadLength = new int[maxSkipLevels];
		}
		
		internal virtual void  Init(long skipPointer, long freqBasePointer, long proxBasePointer, int df, bool storesPayloads)
		{
			base.Init(skipPointer, df);
			this.currentFieldStoresPayloads = storesPayloads;
			lastFreqPointer = freqBasePointer;
			lastProxPointer = proxBasePointer;

			for (int i = 0; i < freqPointer.Length; i++) freqPointer[i] = freqBasePointer;
			for (int i = 0; i < proxPointer.Length; i++) proxPointer[i] = proxBasePointer;
			for (int i = 0; i < payloadLength.Length; i++) payloadLength[i] = 0;
        }
		
		/// <summary>Returns the freq pointer of the doc to which the last call of 
		/// {@link MultiLevelSkipListReader#SkipTo(int)} has skipped.  
		/// </summary>
		internal virtual long GetFreqPointer()
		{
			return lastFreqPointer;
		}
		
		/// <summary>Returns the prox pointer of the doc to which the last call of 
		/// {@link MultiLevelSkipListReader#SkipTo(int)} has skipped.  
		/// </summary>
		internal virtual long GetProxPointer()
		{
			return lastProxPointer;
		}
		
		/// <summary>Returns the payload length of the payload stored just before 
		/// the doc to which the last call of {@link MultiLevelSkipListReader#SkipTo(int)} 
		/// has skipped.  
		/// </summary>
		internal virtual int GetPayloadLength()
		{
			return lastPayloadLength;
		}
		
		protected internal override void  SeekChild(int level)
		{
			base.SeekChild(level);
			freqPointer[level] = lastFreqPointer;
			proxPointer[level] = lastProxPointer;
			payloadLength[level] = lastPayloadLength;
		}
		
		protected internal override void  SetLastSkipData(int level)
		{
			base.SetLastSkipData(level);
			lastFreqPointer = freqPointer[level];
			lastProxPointer = proxPointer[level];
			lastPayloadLength = payloadLength[level];
		}
		
		
		protected internal override int ReadSkipData(int level, IndexInput skipStream)
		{
			int delta;
			if (currentFieldStoresPayloads)
			{
				// the current field stores payloads.
				// if the doc delta is odd then we have
				// to read the current payload length
				// because it differs from the length of the
				// previous payload
				delta = skipStream.ReadVInt();
				if ((delta & 1) != 0)
				{
					payloadLength[level] = skipStream.ReadVInt();
				}
				delta = SupportClass.Number.URShift(delta, 1);
			}
			else
			{
				delta = skipStream.ReadVInt();
			}
			freqPointer[level] += skipStream.ReadVInt();
			proxPointer[level] += skipStream.ReadVInt();
			
			return delta;
		}
	}
}
