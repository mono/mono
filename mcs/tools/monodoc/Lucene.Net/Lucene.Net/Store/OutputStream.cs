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
	
	/// <summary>Abstract class for output to a file in a Directory.  A random-access output
	/// stream.  Used for all Lucene index output operations.
	/// </summary>
	/// <seealso cref="Directory">
	/// </seealso>
	/// <seealso cref="InputStream">
	/// </seealso>
	public abstract class OutputStream
	{
		internal const int BUFFER_SIZE = 1024;
		
		private byte[] buffer = new byte[BUFFER_SIZE];
		private long bufferStart = 0; // position in file of buffer
		private int bufferPosition = 0; // position in buffer
		
		/// <summary>Writes a single byte.</summary>
		/// <seealso cref="InputStream#ReadByte()">
		/// </seealso>
		public void  WriteByte(byte b)
		{
			if (bufferPosition >= BUFFER_SIZE)
				Flush();
			buffer[bufferPosition++] = b;
		}
		
		/// <summary>Writes an array of bytes.</summary>
		/// <param name="b">the bytes to write
		/// </param>
		/// <param name="length">the number of bytes to write
		/// </param>
		/// <seealso cref="InputStream#ReadBytes(byte[],int,int)">
		/// </seealso>
		public void  WriteBytes(byte[] b, int length)
		{
			for (int i = 0; i < length; i++)
				WriteByte(b[i]);
		}
		
		/// <summary>Writes an int as four bytes.</summary>
		/// <seealso cref="InputStream#ReadInt()">
		/// </seealso>
		public void  WriteInt(int i)
		{
			WriteByte((byte) (i >> 24));
			WriteByte((byte) (i >> 16));
			WriteByte((byte) (i >> 8));
			WriteByte((byte) i);
		}
		
		/// <summary>Writes an int in a variable-length format.  Writes between one and
		/// five bytes.  Smaller values take fewer bytes.  Negative numbers are not
		/// supported.
		/// </summary>
		/// <seealso cref="InputStream#ReadVInt()">
		/// </seealso>
		public void  WriteVInt(int i)
		{
			while ((i & ~ 0x7F) != 0)
			{
				WriteByte((byte) ((i & 0x7f) | 0x80));
                i = (int) (((uint) i) >> 7);
			}
			WriteByte((byte) i);
		}
		
		/// <summary>Writes a long as eight bytes.</summary>
		/// <seealso cref="InputStream#ReadLong()">
		/// </seealso>
		public void  WriteLong(long i)
		{
			WriteInt((int) (i >> 32));
			WriteInt((int) i);
		}
		
		/// <summary>Writes an long in a variable-length format.  Writes between one and five
		/// bytes.  Smaller values take fewer bytes.  Negative numbers are not
		/// supported.
		/// </summary>
		/// <seealso cref="InputStream#ReadVLong()">
		/// </seealso>
		public void  WriteVLong(long i)
		{
			while ((i & ~ 0x7F) != 0)
			{
				WriteByte((byte) ((i & 0x7f) | 0x80));
                i = (long) (((long) i) >> 7);
			}
			WriteByte((byte) i);
		}
		
		/// <summary>Writes a string.</summary>
		/// <seealso cref="InputStream#ReadString()">
		/// </seealso>
		public void  WriteString(System.String s)
		{
			int length = s.Length;
			WriteVInt(length);
			WriteChars(s, 0, length);
		}
		
		/// <summary>Writes a sequence of UTF-8 encoded characters from a string.</summary>
		/// <param name="s">the source of the characters
		/// </param>
		/// <param name="start">the first character in the sequence
		/// </param>
		/// <param name="length">the number of characters in the sequence
		/// </param>
		/// <seealso cref="InputStream#ReadChars(char[],int,int)">
		/// </seealso>
		public void  WriteChars(System.String s, int start, int length)
		{
			int end = start + length;
			for (int i = start; i < end; i++)
			{
				int code = (int) s[i];
				if (code >= 0x01 && code <= 0x7F)
					WriteByte((byte) code);
				else if (((code >= 0x80) && (code <= 0x7FF)) || code == 0)
				{
					WriteByte((byte) (0xC0 | (code >> 6)));
					WriteByte((byte) (0x80 | (code & 0x3F)));
				}
				else
				{
					WriteByte((byte) (0xE0 | (((uint) code) >> 12)));
					WriteByte((byte) (0x80 | ((code >> 6) & 0x3F)));
					WriteByte((byte) (0x80 | (code & 0x3F)));
				}
			}
		}
		
		/// <summary>Forces any buffered output to be written. </summary>
		protected internal void  Flush()
		{
			FlushBuffer(buffer, bufferPosition);
			bufferStart += bufferPosition;
			bufferPosition = 0;
		}
		
		/// <summary>Expert: implements buffer write.  Writes bytes at the current position in
		/// the output.
		/// </summary>
		/// <param name="b">the bytes to write
		/// </param>
		/// <param name="len">the number of bytes to write
		/// </param>
		public abstract void  FlushBuffer(byte[] b, int len);
		
		/// <summary>Closes this stream to further operations. </summary>
		public virtual void  Close()
		{
			Flush();
		}
		
		/// <summary>Returns the current position in this file, where the next write will
		/// occur.
		/// </summary>
		/// <seealso cref="#Seek(long)">
		/// </seealso>
		public long GetFilePointer()
		{
			return bufferStart + bufferPosition;
		}
		
		/// <summary>Sets current position in this file, where the next write will occur.</summary>
		/// <seealso cref="#GetFilePointer()">
		/// </seealso>
		public virtual void  Seek(long pos)
		{
			Flush();
			bufferStart = pos;
		}
		
		/// <summary>The number of bytes in the file. </summary>
		public abstract long Length();
	}
}