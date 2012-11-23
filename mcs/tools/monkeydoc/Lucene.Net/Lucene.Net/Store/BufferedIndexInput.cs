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
	
	/// <summary>Base implementation class for buffered {@link IndexInput}. </summary>
	public abstract class BufferedIndexInput:IndexInput, System.ICloneable
	{
		
		/// <summary>Default buffer size </summary>
		public const int BUFFER_SIZE = 1024;
		
		private int bufferSize = BUFFER_SIZE;
		
		protected internal byte[] buffer;
		
		private long bufferStart = 0; // position in file of buffer
		private int bufferLength = 0; // end of valid bytes
		private int bufferPosition = 0; // next byte to read
		
		public override byte ReadByte()
		{
			if (bufferPosition >= bufferLength)
				Refill();
			return buffer[bufferPosition++];
		}
		
		public BufferedIndexInput()
		{
		}
		
		/// <summary>Inits BufferedIndexInput with a specific bufferSize </summary>
		public BufferedIndexInput(int bufferSize)
		{
			CheckBufferSize(bufferSize);
			this.bufferSize = bufferSize;
		}
		
		/// <summary>Change the buffer size used by this IndexInput </summary>
		public virtual void  SetBufferSize(int newSize)
		{
			System.Diagnostics.Debug.Assert(buffer == null || bufferSize == buffer.Length, "buffer=" + buffer + " bufferSize=" + bufferSize + " buffer.length=" +(buffer != null ? buffer.Length: 0));
			if (newSize != bufferSize)
			{
				CheckBufferSize(newSize);
				bufferSize = newSize;
				if (buffer != null)
				{
					// Resize the existing buffer and carefully save as
					// many bytes as possible starting from the current
					// bufferPosition
					byte[] newBuffer = new byte[newSize];
					int leftInBuffer = bufferLength - bufferPosition;
					int numToCopy;
					if (leftInBuffer > newSize)
						numToCopy = newSize;
					else
						numToCopy = leftInBuffer;
					Array.Copy(buffer, bufferPosition, newBuffer, 0, numToCopy);
					bufferStart += bufferPosition;
					bufferPosition = 0;
					bufferLength = numToCopy;
					NewBuffer(newBuffer);
				}
			}
		}
		
		protected internal virtual void  NewBuffer(byte[] newBuffer)
		{
			// Subclasses can do something here
			buffer = newBuffer;
		}
		
		/// <seealso cref="setBufferSize">
		/// </seealso>
		public virtual int GetBufferSize()
		{
			return bufferSize;
		}
		
		private void  CheckBufferSize(int bufferSize)
		{
			if (bufferSize <= 0)
				throw new System.ArgumentException("bufferSize must be greater than 0 (got " + bufferSize + ")");
		}
		
		public override void  ReadBytes(byte[] b, int offset, int len)
		{
			ReadBytes(b, offset, len, true);
		}
		
		public override void  ReadBytes(byte[] b, int offset, int len, bool useBuffer)
		{
			
			if (len <= (bufferLength - bufferPosition))
			{
				// the buffer contains enough data to satisfy this request
				if (len > 0)
				// to allow b to be null if len is 0...
					Array.Copy(buffer, bufferPosition, b, offset, len);
				bufferPosition += len;
			}
			else
			{
				// the buffer does not have enough data. First serve all we've got.
				int available = bufferLength - bufferPosition;
				if (available > 0)
				{
					Array.Copy(buffer, bufferPosition, b, offset, available);
					offset += available;
					len -= available;
					bufferPosition += available;
				}
				// and now, read the remaining 'len' bytes:
				if (useBuffer && len < bufferSize)
				{
					// If the amount left to read is small enough, and
					// we are allowed to use our buffer, do it in the usual
					// buffered way: fill the buffer and copy from it:
					Refill();
					if (bufferLength < len)
					{
						// Throw an exception when refill() could not read len bytes:
						Array.Copy(buffer, 0, b, offset, bufferLength);
						throw new System.IO.IOException("read past EOF");
					}
					else
					{
						Array.Copy(buffer, 0, b, offset, len);
						bufferPosition = len;
					}
				}
				else
				{
					// The amount left to read is larger than the buffer
					// or we've been asked to not use our buffer -
					// there's no performance reason not to read it all
					// at once. Note that unlike the previous code of
					// this function, there is no need to do a seek
					// here, because there's no need to reread what we
					// had in the buffer.
					long after = bufferStart + bufferPosition + len;
					if (after > Length())
						throw new System.IO.IOException("read past EOF");
					ReadInternal(b, offset, len);
					bufferStart = after;
					bufferPosition = 0;
					bufferLength = 0; // trigger refill() on read
				}
			}
		}
		
		private void  Refill()
		{
			long start = bufferStart + bufferPosition;
			long end = start + bufferSize;
			if (end > Length())
			// don't read past EOF
				end = Length();
			int newLength = (int) (end - start);
			if (newLength <= 0)
				throw new System.IO.IOException("read past EOF");
			
			if (buffer == null)
			{
				NewBuffer(new byte[bufferSize]); // allocate buffer lazily
				SeekInternal(bufferStart);
			}
			ReadInternal(buffer, 0, newLength);
			bufferLength = newLength;
			bufferStart = start;
			bufferPosition = 0;
		}
		
		/// <summary>Expert: implements buffer refill.  Reads bytes from the current position
		/// in the input.
		/// </summary>
		/// <param name="b">the array to read bytes into
		/// </param>
		/// <param name="offset">the offset in the array to start storing bytes
		/// </param>
		/// <param name="length">the number of bytes to read
		/// </param>
		public abstract void  ReadInternal(byte[] b, int offset, int length);
		
		public override long GetFilePointer()
		{
			return bufferStart + bufferPosition;
		}
		
		public override void  Seek(long pos)
		{
			if (pos >= bufferStart && pos < (bufferStart + bufferLength))
				bufferPosition = (int) (pos - bufferStart);
			// seek within buffer
			else
			{
				bufferStart = pos;
				bufferPosition = 0;
				bufferLength = 0; // trigger refill() on read()
				SeekInternal(pos);
			}
		}
		
		/// <summary>Expert: implements seek.  Sets current position in this file, where the
		/// next {@link #ReadInternal(byte[],int,int)} will occur.
		/// </summary>
		/// <seealso cref="ReadInternal(byte[],int,int)">
		/// </seealso>
		public abstract void  SeekInternal(long pos);
		
		public override System.Object Clone()
		{
			BufferedIndexInput clone = (BufferedIndexInput) base.Clone();
			
			clone.buffer = null;
			clone.bufferLength = 0;
			clone.bufferPosition = 0;
			clone.bufferStart = GetFilePointer();
			
			return clone;
		}
	}
}
