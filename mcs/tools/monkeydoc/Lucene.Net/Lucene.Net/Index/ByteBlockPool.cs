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


/* Class that Posting and PostingVector use to write byte
* streams into shared fixed-size byte[] arrays.  The idea
* is to allocate slices of increasing lengths For
* example, the first slice is 5 bytes, the next slice is
* 14, etc.  We start by writing our bytes into the first
* 5 bytes.  When we hit the end of the slice, we allocate
* the next slice and then write the address of the new
* slice into the last 4 bytes of the previous slice (the
* "forwarding address").
*
* Each slice is filled with 0's initially, and we mark
* the end with a non-zero byte.  This way the methods
* that are writing into the slice don't need to record
* its length and instead allocate a new slice once they
* hit a non-zero byte. */

using System;
using System.Collections.Generic;

namespace Mono.Lucene.Net.Index
{
	
	sealed public class ByteBlockPool
	{
		private void  InitBlock()
		{
			byteUpto = DocumentsWriter.BYTE_BLOCK_SIZE;
		}
		
		public /*internal*/ abstract class Allocator
		{
			public /*internal*/ abstract void  RecycleByteBlocks(byte[][] blocks, int start, int end);
            public /*internal*/ abstract void RecycleByteBlocks(System.Collections.ArrayList blocks);
			public /*internal*/ abstract byte[] GetByteBlock(bool trackAllocations);
		}
		
		public byte[][] buffers = new byte[10][];
		
		internal int bufferUpto = - 1; // Which buffer we are upto
		public int byteUpto; // Where we are in head buffer
		
		public byte[] buffer; // Current head buffer
		public int byteOffset = - DocumentsWriter.BYTE_BLOCK_SIZE; // Current head offset
		
		private bool trackAllocations;
		private Allocator allocator;
		
		public ByteBlockPool(Allocator allocator, bool trackAllocations)
		{
			InitBlock();
			this.allocator = allocator;
			this.trackAllocations = trackAllocations;
		}
		
		public void  Reset()
		{
			if (bufferUpto != - 1)
			{
				// We allocated at least one buffer
				
				for (int i = 0; i < bufferUpto; i++)
				// Fully zero fill buffers that we fully used
					System.Array.Clear(buffers[i], 0, buffers[i].Length);
				
				// Partial zero fill the final buffer
				System.Array.Clear(buffers[bufferUpto], 0, byteUpto);
				
				if (bufferUpto > 0)
				// Recycle all but the first buffer
					allocator.RecycleByteBlocks(buffers, 1, 1 + bufferUpto);
				
				// Re-use the first buffer
				bufferUpto = 0;
				byteUpto = 0;
				byteOffset = 0;
				buffer = buffers[0];
			}
		}
		
		public void  NextBuffer()
		{
			if (1 + bufferUpto == buffers.Length)
			{
				byte[][] newBuffers = new byte[(int) (buffers.Length * 1.5)][];
				Array.Copy(buffers, 0, newBuffers, 0, buffers.Length);
				buffers = newBuffers;
			}
			buffer = buffers[1 + bufferUpto] = allocator.GetByteBlock(trackAllocations);
			bufferUpto++;
			
			byteUpto = 0;
			byteOffset += DocumentsWriter.BYTE_BLOCK_SIZE;
		}
		
		public int NewSlice(int size)
		{
			if (byteUpto > DocumentsWriter.BYTE_BLOCK_SIZE - size)
				NextBuffer();
			int upto = byteUpto;
			byteUpto += size;
			buffer[byteUpto - 1] = 16;
			return upto;
		}
		
		// Size of each slice.  These arrays should be at most 16
		// elements (index is encoded with 4 bits).  First array
		// is just a compact way to encode X+1 with a max.  Second
		// array is the length of each slice, ie first slice is 5
		// bytes, next slice is 14 bytes, etc.
		internal static readonly int[] nextLevelArray = new int[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 9};
		internal static readonly int[] levelSizeArray = new int[]{5, 14, 20, 30, 40, 40, 80, 80, 120, 200};
		internal static readonly int FIRST_LEVEL_SIZE = levelSizeArray[0];
        public readonly static int FIRST_LEVEL_SIZE_For_NUnit_Test = levelSizeArray[0];
		
		public int AllocSlice(byte[] slice, int upto)
		{
			
			int level = slice[upto] & 15;
			int newLevel = nextLevelArray[level];
			int newSize = levelSizeArray[newLevel];
			
			// Maybe allocate another block
			if (byteUpto > DocumentsWriter.BYTE_BLOCK_SIZE - newSize)
				NextBuffer();
			
			int newUpto = byteUpto;
			int offset = newUpto + byteOffset;
			byteUpto += newSize;
			
			// Copy forward the past 3 bytes (which we are about
			// to overwrite with the forwarding address):
			buffer[newUpto] = slice[upto - 3];
			buffer[newUpto + 1] = slice[upto - 2];
			buffer[newUpto + 2] = slice[upto - 1];
			
			// Write forwarding address at end of last slice:
			slice[upto - 3] = (byte) (SupportClass.Number.URShift(offset, 24));
			slice[upto - 2] = (byte) (SupportClass.Number.URShift(offset, 16));
			slice[upto - 1] = (byte) (SupportClass.Number.URShift(offset, 8));
			slice[upto] = (byte) offset;
			
			// Write new level:
			buffer[byteUpto - 1] = (byte) (16 | newLevel);
			
			return newUpto + 3;
		}

        public static int FIRST_LEVEL_SIZE_ForNUnit
        {
            get { return FIRST_LEVEL_SIZE; }
        }
	}
}
