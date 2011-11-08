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
	
	/* IndexInput that knows how to read the byte slices written
	* by Posting and PostingVector.  We read the bytes in
	* each slice until we hit the end of that slice at which
	* point we read the forwarding address of the next slice
	* and then jump to it.*/
	public sealed class ByteSliceReader:IndexInput
	{
		internal ByteBlockPool pool;
		internal int bufferUpto;
		internal byte[] buffer;
		public int upto;
		internal int limit;
		internal int level;
		public int bufferOffset;
		
		public int endIndex;
		
		public void  Init(ByteBlockPool pool, int startIndex, int endIndex)
		{
			
			System.Diagnostics.Debug.Assert(endIndex - startIndex >= 0);
			System.Diagnostics.Debug.Assert(startIndex >= 0);
			System.Diagnostics.Debug.Assert(endIndex >= 0);
			
			this.pool = pool;
			this.endIndex = endIndex;
			
			level = 0;
			bufferUpto = startIndex / DocumentsWriter.BYTE_BLOCK_SIZE;
			bufferOffset = bufferUpto * DocumentsWriter.BYTE_BLOCK_SIZE;
			buffer = pool.buffers[bufferUpto];
			upto = startIndex & DocumentsWriter.BYTE_BLOCK_MASK;
			
			int firstSize = ByteBlockPool.levelSizeArray[0];
			
			if (startIndex + firstSize >= endIndex)
			{
				// There is only this one slice to read
				limit = endIndex & DocumentsWriter.BYTE_BLOCK_MASK;
			}
			else
				limit = upto + firstSize - 4;
		}
		
		public bool Eof()
		{
			System.Diagnostics.Debug.Assert(upto + bufferOffset <= endIndex);
			return upto + bufferOffset == endIndex;
		}
		
		public override byte ReadByte()
		{
			System.Diagnostics.Debug.Assert(!Eof());
			System.Diagnostics.Debug.Assert(upto <= limit);
			if (upto == limit)
				NextSlice();
			return buffer[upto++];
		}
		
		public long WriteTo(IndexOutput out_Renamed)
		{
			long size = 0;
			while (true)
			{
				if (limit + bufferOffset == endIndex)
				{
					System.Diagnostics.Debug.Assert(endIndex - bufferOffset >= upto);
					out_Renamed.WriteBytes(buffer, upto, limit - upto);
					size += limit - upto;
					break;
				}
				else
				{
					out_Renamed.WriteBytes(buffer, upto, limit - upto);
					size += limit - upto;
					NextSlice();
				}
			}
			
			return size;
		}
		
		public void  NextSlice()
		{
			
			// Skip to our next slice
			int nextIndex = ((buffer[limit] & 0xff) << 24) + ((buffer[1 + limit] & 0xff) << 16) + ((buffer[2 + limit] & 0xff) << 8) + (buffer[3 + limit] & 0xff);
			
			level = ByteBlockPool.nextLevelArray[level];
			int newSize = ByteBlockPool.levelSizeArray[level];
			
			bufferUpto = nextIndex / DocumentsWriter.BYTE_BLOCK_SIZE;
			bufferOffset = bufferUpto * DocumentsWriter.BYTE_BLOCK_SIZE;
			
			buffer = pool.buffers[bufferUpto];
			upto = nextIndex & DocumentsWriter.BYTE_BLOCK_MASK;
			
			if (nextIndex + newSize >= endIndex)
			{
				// We are advancing to the final slice
				System.Diagnostics.Debug.Assert(endIndex - nextIndex > 0);
				limit = endIndex - bufferOffset;
			}
			else
			{
				// This is not the final slice (subtract 4 for the
				// forwarding address at the end of this new slice)
				limit = upto + newSize - 4;
			}
		}
		
		public override void  ReadBytes(byte[] b, int offset, int len)
		{
			while (len > 0)
			{
				int numLeft = limit - upto;
				if (numLeft < len)
				{
					// Read entire slice
					Array.Copy(buffer, upto, b, offset, numLeft);
					offset += numLeft;
					len -= numLeft;
					NextSlice();
				}
				else
				{
					// This slice is the last one
					Array.Copy(buffer, upto, b, offset, len);
					upto += len;
					break;
				}
			}
		}
		
		public override long GetFilePointer()
		{
			throw new System.SystemException("not implemented");
		}
		public override long Length()
		{
			throw new System.SystemException("not implemented");
		}
		public override void  Seek(long pos)
		{
			throw new System.SystemException("not implemented");
		}
		public override void  Close()
		{
			throw new System.SystemException("not implemented");
		}
		
		override public System.Object Clone()
		{
            System.Diagnostics.Debug.Fail("Port issue:", "Let see if we need this ByteSliceReader.Clone()"); // {{Aroush-2.9}}
			return null;
		}
	}
}
