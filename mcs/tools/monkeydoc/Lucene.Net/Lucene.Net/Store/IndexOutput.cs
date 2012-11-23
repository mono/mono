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

using UnicodeUtil = Mono.Lucene.Net.Util.UnicodeUtil;

namespace Mono.Lucene.Net.Store
{
	
	/// <summary>Abstract base class for output to a file in a Directory.  A random-access
	/// output stream.  Used for all Lucene index output operations.
	/// </summary>
	/// <seealso cref="Directory">
	/// </seealso>
	/// <seealso cref="IndexInput">
	/// </seealso>
	public abstract class IndexOutput
	{
		/// <summary>Writes a single byte.</summary>
		/// <seealso cref="IndexInput.ReadByte()">
		/// </seealso>
		public abstract void  WriteByte(byte b);
		
		/// <summary>Writes an array of bytes.</summary>
		/// <param name="b">the bytes to write
		/// </param>
		/// <param name="length">the number of bytes to write
		/// </param>
		/// <seealso cref="IndexInput.ReadBytes(byte[],int,int)">
		/// </seealso>
		public virtual void  WriteBytes(byte[] b, int length)
		{
			WriteBytes(b, 0, length);
		}
		
		/// <summary>Writes an array of bytes.</summary>
		/// <param name="b">the bytes to write
		/// </param>
		/// <param name="offset">the offset in the byte array
		/// </param>
		/// <param name="length">the number of bytes to write
		/// </param>
		/// <seealso cref="IndexInput.ReadBytes(byte[],int,int)">
		/// </seealso>
		public abstract void  WriteBytes(byte[] b, int offset, int length);
		
		/// <summary>Writes an int as four bytes.</summary>
		/// <seealso cref="IndexInput.ReadInt()">
		/// </seealso>
		public virtual void  WriteInt(int i)
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
		/// <seealso cref="IndexInput.ReadVInt()">
		/// </seealso>
		public virtual void  WriteVInt(int i)
		{
			while ((i & ~ 0x7F) != 0)
			{
				WriteByte((byte) ((i & 0x7f) | 0x80));
				i = SupportClass.Number.URShift(i, 7);
			}
			WriteByte((byte) i);
		}
		
		/// <summary>Writes a long as eight bytes.</summary>
		/// <seealso cref="IndexInput.ReadLong()">
		/// </seealso>
		public virtual void  WriteLong(long i)
		{
			WriteInt((int) (i >> 32));
			WriteInt((int) i);
		}
		
		/// <summary>Writes an long in a variable-length format.  Writes between one and five
		/// bytes.  Smaller values take fewer bytes.  Negative numbers are not
		/// supported.
		/// </summary>
		/// <seealso cref="IndexInput.ReadVLong()">
		/// </seealso>
		public virtual void  WriteVLong(long i)
		{
			while ((i & ~ 0x7F) != 0)
			{
				WriteByte((byte) ((i & 0x7f) | 0x80));
				i = SupportClass.Number.URShift(i, 7);
			}
			WriteByte((byte) i);
		}
		
		/// <summary>Writes a string.</summary>
		/// <seealso cref="IndexInput.ReadString()">
		/// </seealso>
		public virtual void  WriteString(System.String s)
		{
            UnicodeUtil.UTF8Result utf8Result = new UnicodeUtil.UTF8Result();
			UnicodeUtil.UTF16toUTF8(s, 0, s.Length, utf8Result);
			WriteVInt(utf8Result.length);
			WriteBytes(utf8Result.result, 0, utf8Result.length);
		}
		
		/// <summary>Writes a sub sequence of characters from s as the old
		/// format (modified UTF-8 encoded bytes).
		/// </summary>
		/// <param name="s">the source of the characters
		/// </param>
		/// <param name="start">the first character in the sequence
		/// </param>
		/// <param name="length">the number of characters in the sequence
		/// </param>
		/// <deprecated> -- please pre-convert to utf8 bytes
		/// instead or use {@link #writeString}
		/// </deprecated>
        [Obsolete("-- please pre-convert to utf8 bytes instead or use WriteString")]
		public virtual void  WriteChars(System.String s, int start, int length)
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
					WriteByte((byte) (0xE0 | (SupportClass.Number.URShift(code, 12))));
					WriteByte((byte) (0x80 | ((code >> 6) & 0x3F)));
					WriteByte((byte) (0x80 | (code & 0x3F)));
				}
			}
		}
		
		/// <summary>Writes a sub sequence of characters from char[] as
		/// the old format (modified UTF-8 encoded bytes).
		/// </summary>
		/// <param name="s">the source of the characters
		/// </param>
		/// <param name="start">the first character in the sequence
		/// </param>
		/// <param name="length">the number of characters in the sequence
		/// </param>
		/// <deprecated> -- please pre-convert to utf8 bytes instead or use {@link #writeString}
		/// </deprecated>
        [Obsolete("-- please pre-convert to utf8 bytes instead or use WriteString")]
		public virtual void  WriteChars(char[] s, int start, int length)
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
					WriteByte((byte) (0xE0 | (SupportClass.Number.URShift(code, 12))));
					WriteByte((byte) (0x80 | ((code >> 6) & 0x3F)));
					WriteByte((byte) (0x80 | (code & 0x3F)));
				}
			}
		}
		
		private static int COPY_BUFFER_SIZE = 16384;
		private byte[] copyBuffer;
		
		/// <summary>Copy numBytes bytes from input to ourself. </summary>
		public virtual void  CopyBytes(IndexInput input, long numBytes)
		{
			System.Diagnostics.Debug.Assert(numBytes >= 0, "numBytes=" + numBytes);
			long left = numBytes;
			if (copyBuffer == null)
				copyBuffer = new byte[COPY_BUFFER_SIZE];
			while (left > 0)
			{
				int toCopy;
				if (left > COPY_BUFFER_SIZE)
					toCopy = COPY_BUFFER_SIZE;
				else
					toCopy = (int) left;
				input.ReadBytes(copyBuffer, 0, toCopy);
				WriteBytes(copyBuffer, 0, toCopy);
				left -= toCopy;
			}
		}
		
		/// <summary>Forces any buffered output to be written. </summary>
		public abstract void  Flush();
		
		/// <summary>Closes this stream to further operations. </summary>
		public abstract void  Close();
		
		/// <summary>Returns the current position in this file, where the next write will
		/// occur.
		/// </summary>
		/// <seealso cref="Seek(long)">
		/// </seealso>
		public abstract long GetFilePointer();
		
		/// <summary>Sets current position in this file, where the next write will occur.</summary>
		/// <seealso cref="GetFilePointer()">
		/// </seealso>
		public abstract void  Seek(long pos);
		
		/// <summary>The number of bytes in the file. </summary>
		public abstract long Length();
		
		/// <summary>Set the file length. By default, this method does
		/// nothing (it's optional for a Directory to implement
		/// it).  But, certain Directory implementations (for
		/// </summary>
		/// <seealso cref="FSDirectory"> can use this to inform the
		/// underlying IO system to pre-allocate the file to the
		/// specified size.  If the length is longer than the
		/// current file length, the bytes added to the file are
		/// undefined.  Otherwise the file is truncated.
		/// </seealso>
		/// <param name="length">file length
		/// </param>
		public virtual void  SetLength(long length)
		{
		}
		
		
		// map must be Map<String, String>
		public virtual void  WriteStringStringMap(System.Collections.Generic.IDictionary<string,string> map)
		{
			if (map == null)
			{
				WriteInt(0);
			}
			else
			{
				WriteInt(map.Count);
                foreach (string key in map.Keys)
                {
                    WriteString(key);
                    WriteString(map[key]);
                }
			}
		}
	}
}
