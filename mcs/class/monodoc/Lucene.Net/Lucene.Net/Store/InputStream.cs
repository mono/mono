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
	
	/// <summary>Abstract base class for input from a file in a {@link Directory}.  A
	/// random-access input stream.  Used for all Lucene index input operations.
	/// </summary>
	/// <seealso cref="Directory">
	/// </seealso>
	/// <seealso cref="OutputStream">
	/// </seealso>
	public abstract class InputStream : System.ICloneable
	{
		internal static readonly int BUFFER_SIZE;
		
		private byte[] buffer;
		private char[] chars;
		
		private long bufferStart = 0; // position in file of buffer
		private int bufferLength = 0; // end of valid bytes
		private int bufferPosition = 0; // next byte to read
		
		protected internal long length; // set by subclasses
		
		/// <summary>Reads and returns a single byte.</summary>
		/// <seealso cref="OutputStream#WriteByte(byte)">
		/// </seealso>
		public byte ReadByte()
		{
			if (bufferPosition >= bufferLength)
				Refill();
			return buffer[bufferPosition++];
		}
		
		/// <summary>Reads a specified number of bytes into an array at the specified offset.</summary>
		/// <param name="b">the array to read bytes into
		/// </param>
		/// <param name="offset">the offset in the array to start storing bytes
		/// </param>
		/// <param name="len">the number of bytes to read
		/// </param>
		/// <seealso cref="OutputStream#WriteBytes(byte[],int)">
		/// </seealso>
		public void  ReadBytes(byte[] b, int offset, int len)
		{
			if (len < BUFFER_SIZE)
			{
				for (int i = 0; i < len; i++)
				// read byte-by-byte
					b[i + offset] = (byte) ReadByte();
			}
			else
			{
				// read all-at-once
				long start = GetFilePointer();
				SeekInternal(start);
				ReadInternal(b, offset, len);
				
				bufferStart = start + len; // adjust stream variables
				bufferPosition = 0;
				bufferLength = 0; // trigger refill() on read
			}
		}
		
		/// <summary>Reads four bytes and returns an int.</summary>
		/// <seealso cref="OutputStream#WriteInt(int)">
		/// </seealso>
		public int ReadInt()
		{
			return ((ReadByte() & 0xFF) << 24) | ((ReadByte() & 0xFF) << 16) | ((ReadByte() & 0xFF) << 8) | (ReadByte() & 0xFF);
		}
		
		/// <summary>Reads an int stored in variable-length format.  Reads between one and
		/// five bytes.  Smaller values take fewer bytes.  Negative numbers are not
		/// supported.
		/// </summary>
		/// <seealso cref="OutputStream#WriteVInt(int)">
		/// </seealso>
		public int ReadVInt()
		{
			byte b = ReadByte();
			int i = b & 0x7F;
			for (int shift = 7; (b & 0x80) != 0; shift += 7)
			{
				b = ReadByte();
				i |= (b & 0x7F) << shift;
			}
			return i;
		}
		
		/// <summary>Reads eight bytes and returns a long.</summary>
		/// <seealso cref="OutputStream#WriteLong(long)">
		/// </seealso>
		public long ReadLong()
		{
			return (((long) ReadInt()) << 32) | (ReadInt() & 0xFFFFFFFFL);
		}
		
		/// <summary>Reads a long stored in variable-length format.  Reads between one and
		/// nine bytes.  Smaller values take fewer bytes.  Negative numbers are not
		/// supported. 
		/// </summary>
		public long ReadVLong()
		{
			byte b = ReadByte();
			long i = b & 0x7F;
			for (int shift = 7; (b & 0x80) != 0; shift += 7)
			{
				b = ReadByte();
				i |= (b & 0x7FL) << shift;
			}
			return i;
		}
		
		/// <summary>Reads a string.</summary>
		/// <seealso cref="OutputStream#WriteString(String)">
		/// </seealso>
		public System.String ReadString()
		{
			int length = ReadVInt();
			if (chars == null || length > chars.Length)
				chars = new char[length];
			ReadChars(chars, 0, length);
			return new System.String(chars, 0, length);
		}
		
		/// <summary>Reads UTF-8 encoded characters into an array.</summary>
		/// <param name="buffer">the array to read characters into
		/// </param>
		/// <param name="start">the offset in the array to start storing characters
		/// </param>
		/// <param name="length">the number of characters to read
		/// </param>
		/// <seealso cref="OutputStream#WriteChars(String,int,int)">
		/// </seealso>
		public void  ReadChars(char[] buffer, int start, int length)
		{
			int end = start + length;
			for (int i = start; i < end; i++)
			{
				byte b = ReadByte();
				if ((b & 0x80) == 0)
					buffer[i] = (char) (b & 0x7F);
				else if ((b & 0xE0) != 0xE0)
				{
					buffer[i] = (char) (((b & 0x1F) << 6) | (ReadByte() & 0x3F));
				}
				else
					buffer[i] = (char) (((b & 0x0F) << 12) | ((ReadByte() & 0x3F) << 6) | (ReadByte() & 0x3F));
			}
		}
		
		
		private void  Refill()
		{
			long start = bufferStart + bufferPosition;
			long end = start + BUFFER_SIZE;
			if (end > length)
			// don't read past EOF
				end = length;
			bufferLength = (int) (end - start);
			if (bufferLength == 0)
				throw new System.IO.IOException("read past EOF");
			
			if (buffer == null)
				buffer = new byte[BUFFER_SIZE]; // allocate buffer lazily
			ReadInternal(buffer, 0, bufferLength);
			
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
		
		/// <summary>Closes the stream to futher operations. </summary>
		public abstract void  Close();
		
		/// <summary>Returns the current position in this file, where the next read will
		/// occur.
		/// </summary>
		/// <seealso cref="#Seek(long)">
		/// </seealso>
		public long GetFilePointer()
		{
			return bufferStart + bufferPosition;
		}
		
		/// <summary>Sets current position in this file, where the next read will occur.</summary>
		/// <seealso cref="#GetFilePointer()">
		/// </seealso>
		public void  Seek(long pos)
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
		/// <seealso cref="#ReadInternal(byte[],int,int)">
		/// </seealso>
		public abstract void  SeekInternal(long pos);
		
		/// <summary>The number of bytes in the file. </summary>
		public long Length()
		{
			return length;
		}
		
		/// <summary>Returns a clone of this stream.
		/// 
		/// <p>Clones of a stream access the same data, and are positioned at the same
		/// point as the stream they were cloned from.
		/// 
		/// <p>Expert: Subclasses must ensure that clones may be positioned at
		/// different points in the input from each other and from the stream they
		/// were cloned from.
		/// </summary>
		public virtual System.Object Clone()
		{
			InputStream clone = null;
			try
			{
				clone = (InputStream) this.MemberwiseClone();
			}
			catch (System.Exception e)
			{
                throw new Exception("Can't clone InputStream.", e);
			}
			
			if (buffer != null)
			{
				clone.buffer = new byte[BUFFER_SIZE];
				Array.Copy(buffer, 0, clone.buffer, 0, bufferLength);
			}
			
			clone.chars = null;
			
			return clone;
		}

		static InputStream()
		{
			BUFFER_SIZE = OutputStream.BUFFER_SIZE;
		}
	}
}