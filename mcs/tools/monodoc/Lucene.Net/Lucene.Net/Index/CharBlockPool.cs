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
	
	sealed class CharBlockPool
	{
		private void  InitBlock()
		{
			charUpto = DocumentsWriter.CHAR_BLOCK_SIZE;
		}
		
		public char[][] buffers = new char[10][];
		internal int numBuffer;
		
		internal int bufferUpto = - 1; // Which buffer we are upto
		public int charUpto; // Where we are in head buffer
		
		public char[] buffer; // Current head buffer
		public int charOffset = - DocumentsWriter.CHAR_BLOCK_SIZE; // Current head offset
		private DocumentsWriter docWriter;
		
		public CharBlockPool(DocumentsWriter docWriter)
		{
			InitBlock();
			this.docWriter = docWriter;
		}
		
		public void  Reset()
		{
			docWriter.RecycleCharBlocks(buffers, 1 + bufferUpto);
			bufferUpto = - 1;
			charUpto = DocumentsWriter.CHAR_BLOCK_SIZE;
			charOffset = - DocumentsWriter.CHAR_BLOCK_SIZE;
		}
		
		public void  NextBuffer()
		{
			if (1 + bufferUpto == buffers.Length)
			{
				char[][] newBuffers = new char[(int) (buffers.Length * 1.5)][];
				Array.Copy(buffers, 0, newBuffers, 0, buffers.Length);
				buffers = newBuffers;
			}
			buffer = buffers[1 + bufferUpto] = docWriter.GetCharBlock();
			bufferUpto++;
			
			charUpto = 0;
			charOffset += DocumentsWriter.CHAR_BLOCK_SIZE;
		}
	}
}
