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

namespace Mono.Lucene.Net.Store
{
	
	/// <summary> A memory-resident {@link IndexOutput} implementation.
	/// 
	/// </summary>
	/// <version>  $Id: RAMOutputStream.java 691694 2008-09-03 17:34:29Z mikemccand $
	/// </version>
	
	public class RAMOutputStream:IndexOutput
	{
		internal const int BUFFER_SIZE = 1024;
		
		private RAMFile file;
		
		private byte[] currentBuffer;
		private int currentBufferIndex;
		
		private int bufferPosition;
		private long bufferStart;
		private int bufferLength;
		
		/// <summary>Construct an empty output buffer. </summary>
		public RAMOutputStream():this(new RAMFile())
		{
		}
		
		public /*internal*/ RAMOutputStream(RAMFile f)
		{
			file = f;
			
			// make sure that we switch to the
			// first needed buffer lazily
			currentBufferIndex = - 1;
			currentBuffer = null;
		}
		
		/// <summary>Copy the current contents of this buffer to the named output. </summary>
		public virtual void  WriteTo(IndexOutput out_Renamed)
		{
			Flush();
			long end = file.length;
			long pos = 0;
			int buffer = 0;
			while (pos < end)
			{
				int length = BUFFER_SIZE;
				long nextPos = pos + length;
				if (nextPos > end)
				{
					// at the last buffer
					length = (int) (end - pos);
				}
				out_Renamed.WriteBytes((byte[]) file.GetBuffer(buffer++), length);
				pos = nextPos;
			}
		}
		
		/// <summary>Resets this to an empty buffer. </summary>
		public virtual void  Reset()
		{
            currentBuffer = null;
            currentBufferIndex = -1;
            bufferPosition = 0;
            bufferStart = 0;
            bufferLength = 0;
			
			file.SetLength(0);
		}
		
		public override void  Close()
		{
			Flush();
		}
		
		public override void  Seek(long pos)
		{
			// set the file length in case we seek back
			// and flush() has not been called yet
			SetFileLength();
			if (pos < bufferStart || pos >= bufferStart + bufferLength)
			{
				currentBufferIndex = (int) (pos / BUFFER_SIZE);
				SwitchCurrentBuffer();
			}
			
			bufferPosition = (int) (pos % BUFFER_SIZE);
		}
		
		public override long Length()
		{
			return file.length;
		}
		
		public override void  WriteByte(byte b)
		{
			if (bufferPosition == bufferLength)
			{
				currentBufferIndex++;
				SwitchCurrentBuffer();
			}
			currentBuffer[bufferPosition++] = b;
		}
		
		public override void  WriteBytes(byte[] b, int offset, int len)
		{
			System.Diagnostics.Debug.Assert(b != null);
			while (len > 0)
			{
				if (bufferPosition == bufferLength)
				{
					currentBufferIndex++;
					SwitchCurrentBuffer();
				}
				
				int remainInBuffer = currentBuffer.Length - bufferPosition;
				int bytesToCopy = len < remainInBuffer?len:remainInBuffer;
				Array.Copy(b, offset, currentBuffer, bufferPosition, bytesToCopy);
				offset += bytesToCopy;
				len -= bytesToCopy;
				bufferPosition += bytesToCopy;
			}
		}
		
		private void  SwitchCurrentBuffer()
		{
			if (currentBufferIndex == file.NumBuffers())
			{
				currentBuffer = file.AddBuffer(BUFFER_SIZE);
			}
			else
			{
				currentBuffer = (byte[]) file.GetBuffer(currentBufferIndex);
			}
			bufferPosition = 0;
			bufferStart = (long) BUFFER_SIZE * (long) currentBufferIndex;
			bufferLength = currentBuffer.Length;
		}
		
		private void  SetFileLength()
		{
			long pointer = bufferStart + bufferPosition;
			if (pointer > file.length)
			{
				file.SetLength(pointer);
			}
		}
		
		public override void  Flush()
		{
			file.SetLastModified((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond));
			SetFileLength();
		}
		
		public override long GetFilePointer()
		{
			return currentBufferIndex < 0?0:bufferStart + bufferPosition;
		}
		
		/// <summary>Returns byte usage of all buffers. </summary>
		public virtual long SizeInBytes()
		{
			return file.NumBuffers() * BUFFER_SIZE;
		}

        public static int BUFFER_SIZE_ForNUnit
        {
            get { return BUFFER_SIZE; }
        }
	}
}
