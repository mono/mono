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
namespace Monodoc.Lucene.Net.Store
{
	/// <summary> A memory-resident {@link InputStream} implementation.
	/// 
	/// </summary>
	/// <version>  $Id: RAMInputStream.java,v 1.2 2004/03/29 22:48:05 cutting Exp $
	/// </version>
	
	class RAMInputStream:InputStream, System.ICloneable
	{
		private RAMFile file;
		private int pointer = 0;
		
		public RAMInputStream(RAMFile f)
		{
			file = f;
			length = file.length;
		}
		
		public override void  ReadInternal(byte[] dest, int destOffset, int len)
		{
			int remainder = len;
			int start = pointer;
			while (remainder != 0)
			{
				int bufferNumber = start / BUFFER_SIZE;
				int bufferOffset = start % BUFFER_SIZE;
				int bytesInBuffer = BUFFER_SIZE - bufferOffset;
				int bytesToCopy = bytesInBuffer >= remainder?remainder:bytesInBuffer;
				byte[] buffer = (byte[]) file.buffers[bufferNumber];
				Array.Copy(buffer, bufferOffset, dest, destOffset, bytesToCopy);
				destOffset += bytesToCopy;
				start += bytesToCopy;
				remainder -= bytesToCopy;
			}
			pointer += len;
		}
		
		public override void  Close()
		{
		}
		
		public override void  SeekInternal(long pos)
		{
			pointer = (int) pos;
		}
        /*
		override public System.Object Clone()
		{
			return null;
		}
        */
	}
}