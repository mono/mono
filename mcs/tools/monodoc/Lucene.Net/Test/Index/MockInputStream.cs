/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
using InputStream = Lucene.Net.Store.InputStream;
namespace Lucene.Net.Index
{
	
	public class MockInputStream : InputStream
	{
		private byte[] buffer;
		private int pointer = 0;
		
		public MockInputStream(byte[] bytes)
		{
			buffer = bytes;
			length = bytes.Length;
		}
		
		public override void  ReadInternal(byte[] dest, int destOffset, int len)
		{
			int remainder = len;
			int start = pointer;
			while (remainder != 0)
			{
				//          int bufferNumber = start / buffer.length;
				int bufferOffset = start % buffer.Length;
				int bytesInBuffer = buffer.Length - bufferOffset;
				int bytesToCopy = bytesInBuffer >= remainder?remainder:bytesInBuffer;
				Array.Copy(buffer, bufferOffset, dest, destOffset, bytesToCopy);
				destOffset += bytesToCopy;
				start += bytesToCopy;
				remainder -= bytesToCopy;
			}
			pointer += len;
		}
		
		public override void  Close()
		{
			// ignore
		}
		
		public override void  SeekInternal(long pos)
		{
			pointer = (int) pos;
		}
	}
}