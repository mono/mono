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
	
	/// <summary> Class to write byte streams into slices of shared
	/// byte[].  This is used by DocumentsWriter to hold the
	/// posting list for many terms in RAM.
	/// </summary>
	
	public sealed class ByteSliceWriter
	{
		
		private byte[] slice;
		private int upto;
		private ByteBlockPool pool;
		
		internal int offset0;
		
		public ByteSliceWriter(ByteBlockPool pool)
		{
			this.pool = pool;
		}
		
		/// <summary> Set up the writer to write at address.</summary>
		public void  Init(int address)
		{
			slice = pool.buffers[address >> DocumentsWriter.BYTE_BLOCK_SHIFT];
			System.Diagnostics.Debug.Assert(slice != null);
			upto = address & DocumentsWriter.BYTE_BLOCK_MASK;
			offset0 = address;
			System.Diagnostics.Debug.Assert(upto < slice.Length);
		}
		
		/// <summary>Write byte into byte slice stream </summary>
		public void  WriteByte(byte b)
		{
			System.Diagnostics.Debug.Assert(slice != null);
			if (slice[upto] != 0)
			{
				upto = pool.AllocSlice(slice, upto);
				slice = pool.buffer;
				offset0 = pool.byteOffset;
				System.Diagnostics.Debug.Assert(slice != null);
			}
			slice[upto++] = b;
			System.Diagnostics.Debug.Assert(upto != slice.Length);
		}
		
		public void  WriteBytes(byte[] b, int offset, int len)
		{
			int offsetEnd = offset + len;
			while (offset < offsetEnd)
			{
				if (slice[upto] != 0)
				{
					// End marker
					upto = pool.AllocSlice(slice, upto);
					slice = pool.buffer;
					offset0 = pool.byteOffset;
				}
				
				slice[upto++] = b[offset++];
				System.Diagnostics.Debug.Assert(upto != slice.Length);
			}
		}
		
		public int GetAddress()
		{
			return upto + (offset0 & DocumentsWriter.BYTE_BLOCK_NOT_MASK);
		}
		
		public void  WriteVInt(int i)
		{
			while ((i & ~ 0x7F) != 0)
			{
				WriteByte((byte) ((i & 0x7f) | 0x80));
				i = SupportClass.Number.URShift(i, 7);
			}
			WriteByte((byte) i);
		}
	}
}
