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
	
	public sealed class SegmentTermPositions:SegmentTermDocs, TermPositions
	{
		private IndexInput proxStream;
		private int proxCount;
		private int position;
		
		// the current payload length
		private int payloadLength;
		// indicates whether the payload of the currend position has
		// been read from the proxStream yet
		private bool needToLoadPayload;
		
		// these variables are being used to remember information
		// for a lazy skip
		private long lazySkipPointer = - 1;
		private int lazySkipProxCount = 0;
		
		internal SegmentTermPositions(SegmentReader p):base(p)
		{
			this.proxStream = null; // the proxStream will be cloned lazily when nextPosition() is called for the first time
		}
		
		internal override void  Seek(TermInfo ti, Term term)
		{
			base.Seek(ti, term);
			if (ti != null)
				lazySkipPointer = ti.proxPointer;
			
			lazySkipProxCount = 0;
			proxCount = 0;
			payloadLength = 0;
			needToLoadPayload = false;
		}
		
		public override void  Close()
		{
			base.Close();
			if (proxStream != null)
				proxStream.Close();
		}
		
		public int NextPosition()
		{
			if (currentFieldOmitTermFreqAndPositions)
			// This field does not store term freq, positions, payloads
				return 0;
			// perform lazy skips if neccessary
			LazySkip();
			proxCount--;
			return position += ReadDeltaPosition();
		}
		
		private int ReadDeltaPosition()
		{
			int delta = proxStream.ReadVInt();
			if (currentFieldStoresPayloads)
			{
				// if the current field stores payloads then
				// the position delta is shifted one bit to the left.
				// if the LSB is set, then we have to read the current
				// payload length
				if ((delta & 1) != 0)
				{
					payloadLength = proxStream.ReadVInt();
				}
				delta = SupportClass.Number.URShift(delta, 1);
				needToLoadPayload = true;
			}
			return delta;
		}
		
		protected internal override void  SkippingDoc()
		{
			// we remember to skip a document lazily
			lazySkipProxCount += freq;
		}
		
		public override bool Next()
		{
			// we remember to skip the remaining positions of the current
			// document lazily
			lazySkipProxCount += proxCount;
			
			if (base.Next())
			{
				// run super
				proxCount = freq; // note frequency
				position = 0; // reset position
				return true;
			}
			return false;
		}
		
		public override int Read(int[] docs, int[] freqs)
		{
			throw new System.NotSupportedException("TermPositions does not support processing multiple documents in one call. Use TermDocs instead.");
		}
		
		
		/// <summary>Called by super.skipTo(). </summary>
		protected internal override void  SkipProx(long proxPointer, int payloadLength)
		{
			// we save the pointer, we might have to skip there lazily
			lazySkipPointer = proxPointer;
			lazySkipProxCount = 0;
			proxCount = 0;
			this.payloadLength = payloadLength;
			needToLoadPayload = false;
		}
		
		private void  SkipPositions(int n)
		{
			System.Diagnostics.Debug.Assert(!currentFieldOmitTermFreqAndPositions);
			for (int f = n; f > 0; f--)
			{
				// skip unread positions
				ReadDeltaPosition();
				SkipPayload();
			}
		}
		
		private void  SkipPayload()
		{
			if (needToLoadPayload && payloadLength > 0)
			{
				proxStream.Seek(proxStream.GetFilePointer() + payloadLength);
			}
			needToLoadPayload = false;
		}
		
		// It is not always neccessary to move the prox pointer
		// to a new document after the freq pointer has been moved.
		// Consider for example a phrase query with two terms:
		// the freq pointer for term 1 has to move to document x
		// to answer the question if the term occurs in that document. But
		// only if term 2 also matches document x, the positions have to be
		// read to figure out if term 1 and term 2 appear next
		// to each other in document x and thus satisfy the query.
		// So we move the prox pointer lazily to the document
		// as soon as positions are requested.
		private void  LazySkip()
		{
			if (proxStream == null)
			{
				// clone lazily
				proxStream = (IndexInput) parent.core.proxStream.Clone();
			}
			
			// we might have to skip the current payload
			// if it was not read yet
			SkipPayload();
			
			if (lazySkipPointer != - 1)
			{
				proxStream.Seek(lazySkipPointer);
				lazySkipPointer = - 1;
			}
			
			if (lazySkipProxCount != 0)
			{
				SkipPositions(lazySkipProxCount);
				lazySkipProxCount = 0;
			}
		}
		
		public int GetPayloadLength()
		{
			return payloadLength;
		}
		
		public byte[] GetPayload(byte[] data, int offset)
		{
			if (!needToLoadPayload)
			{
				throw new System.IO.IOException("Either no payload exists at this term position or an attempt was made to load it more than once.");
			}
			
			// read payloads lazily
			byte[] retArray;
			int retOffset;
			if (data == null || data.Length - offset < payloadLength)
			{
				// the array is too small to store the payload data,
				// so we allocate a new one
				retArray = new byte[payloadLength];
				retOffset = 0;
			}
			else
			{
				retArray = data;
				retOffset = offset;
			}
			proxStream.ReadBytes(retArray, retOffset, payloadLength);
			needToLoadPayload = false;
			return retArray;
		}
		
		public bool IsPayloadAvailable()
		{
			return needToLoadPayload && payloadLength > 0;
		}
	}
}
