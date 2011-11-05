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

using ArrayUtil = Mono.Lucene.Net.Util.ArrayUtil;
using AttributeImpl = Mono.Lucene.Net.Util.AttributeImpl;

namespace Mono.Lucene.Net.Analysis.Tokenattributes
{
	
	/// <summary> The term text of a Token.</summary>
	[Serializable]
	public class TermAttributeImpl:AttributeImpl, TermAttribute, System.ICloneable
	{
		private static int MIN_BUFFER_SIZE = 10;
		
		private char[] termBuffer;
		private int termLength;
		
		/// <summary>Returns the Token's term text.
		/// 
		/// This method has a performance penalty
		/// because the text is stored internally in a char[].  If
		/// possible, use {@link #TermBuffer()} and {@link
		/// #TermLength()} directly instead.  If you really need a
		/// String, use this method, which is nothing more than
		/// a convenience call to <b>new String(token.termBuffer(), 0, token.termLength())</b>
		/// </summary>
		public virtual System.String Term()
		{
			InitTermBuffer();
			return new System.String(termBuffer, 0, termLength);
		}
		
		/// <summary>Copies the contents of buffer, starting at offset for
		/// length characters, into the termBuffer array.
		/// </summary>
		/// <param name="buffer">the buffer to copy
		/// </param>
		/// <param name="offset">the index in the buffer of the first character to copy
		/// </param>
		/// <param name="length">the number of characters to copy
		/// </param>
		public virtual void  SetTermBuffer(char[] buffer, int offset, int length)
		{
			GrowTermBuffer(length);
			Array.Copy(buffer, offset, termBuffer, 0, length);
			termLength = length;
		}
		
		/// <summary>Copies the contents of buffer into the termBuffer array.</summary>
		/// <param name="buffer">the buffer to copy
		/// </param>
		public virtual void  SetTermBuffer(System.String buffer)
		{
			int length = buffer.Length;
			GrowTermBuffer(length);
			SupportClass.TextSupport.GetCharsFromString(buffer, 0, length, termBuffer, 0);
			termLength = length;
		}
		
		/// <summary>Copies the contents of buffer, starting at offset and continuing
		/// for length characters, into the termBuffer array.
		/// </summary>
		/// <param name="buffer">the buffer to copy
		/// </param>
		/// <param name="offset">the index in the buffer of the first character to copy
		/// </param>
		/// <param name="length">the number of characters to copy
		/// </param>
		public virtual void  SetTermBuffer(System.String buffer, int offset, int length)
		{
			System.Diagnostics.Debug.Assert(offset <= buffer.Length);
			System.Diagnostics.Debug.Assert(offset + length <= buffer.Length);
			GrowTermBuffer(length);
			SupportClass.TextSupport.GetCharsFromString(buffer, offset, offset + length, termBuffer, 0);
			termLength = length;
		}
		
		/// <summary>Returns the internal termBuffer character array which
		/// you can then directly alter.  If the array is too
		/// small for your token, use {@link
		/// #ResizeTermBuffer(int)} to increase it.  After
		/// altering the buffer be sure to call {@link
		/// #setTermLength} to record the number of valid
		/// characters that were placed into the termBuffer. 
		/// </summary>
		public virtual char[] TermBuffer()
		{
			InitTermBuffer();
			return termBuffer;
		}
		
		/// <summary>Grows the termBuffer to at least size newSize, preserving the
		/// existing content. Note: If the next operation is to change
		/// the contents of the term buffer use
		/// {@link #SetTermBuffer(char[], int, int)},
		/// {@link #SetTermBuffer(String)}, or
		/// {@link #SetTermBuffer(String, int, int)}
		/// to optimally combine the resize with the setting of the termBuffer.
		/// </summary>
		/// <param name="newSize">minimum size of the new termBuffer
		/// </param>
		/// <returns> newly created termBuffer with length >= newSize
		/// </returns>
		public virtual char[] ResizeTermBuffer(int newSize)
		{
			if (termBuffer == null)
			{
				// The buffer is always at least MIN_BUFFER_SIZE
				termBuffer = new char[ArrayUtil.GetNextSize(newSize < MIN_BUFFER_SIZE?MIN_BUFFER_SIZE:newSize)];
			}
			else
			{
				if (termBuffer.Length < newSize)
				{
					// Not big enough; create a new array with slight
					// over allocation and preserve content
					char[] newCharBuffer = new char[ArrayUtil.GetNextSize(newSize)];
					Array.Copy(termBuffer, 0, newCharBuffer, 0, termBuffer.Length);
					termBuffer = newCharBuffer;
				}
			}
			return termBuffer;
		}
		
		
		/// <summary>Allocates a buffer char[] of at least newSize, without preserving the existing content.
		/// its always used in places that set the content 
		/// </summary>
		/// <param name="newSize">minimum size of the buffer
		/// </param>
		private void  GrowTermBuffer(int newSize)
		{
			if (termBuffer == null)
			{
				// The buffer is always at least MIN_BUFFER_SIZE
				termBuffer = new char[ArrayUtil.GetNextSize(newSize < MIN_BUFFER_SIZE?MIN_BUFFER_SIZE:newSize)];
			}
			else
			{
				if (termBuffer.Length < newSize)
				{
					// Not big enough; create a new array with slight
					// over allocation:
					termBuffer = new char[ArrayUtil.GetNextSize(newSize)];
				}
			}
		}
		
		private void  InitTermBuffer()
		{
			if (termBuffer == null)
			{
				termBuffer = new char[ArrayUtil.GetNextSize(MIN_BUFFER_SIZE)];
				termLength = 0;
			}
		}
		
		/// <summary>Return number of valid characters (length of the term)
		/// in the termBuffer array. 
		/// </summary>
		public virtual int TermLength()
		{
			return termLength;
		}
		
		/// <summary>Set number of valid characters (length of the term) in
		/// the termBuffer array. Use this to truncate the termBuffer
		/// or to synchronize with external manipulation of the termBuffer.
		/// Note: to grow the size of the array,
		/// use {@link #ResizeTermBuffer(int)} first.
		/// </summary>
		/// <param name="length">the truncated length
		/// </param>
		public virtual void  SetTermLength(int length)
		{
			InitTermBuffer();
			if (length > termBuffer.Length)
				throw new System.ArgumentException("length " + length + " exceeds the size of the termBuffer (" + termBuffer.Length + ")");
			termLength = length;
		}
		
		public override int GetHashCode()
		{
			InitTermBuffer();
			int code = termLength;
			code = code * 31 + ArrayUtil.HashCode(termBuffer, 0, termLength);
			return code;
		}
		
		public override void  Clear()
		{
			termLength = 0;
		}
		
		public override System.Object Clone()
		{
			TermAttributeImpl t = (TermAttributeImpl) base.Clone();
			// Do a deep clone
			if (termBuffer != null)
			{
				t.termBuffer = new char[termBuffer.Length];
				termBuffer.CopyTo(t.termBuffer, 0);
			}
			return t;
		}
		
		public  override bool Equals(System.Object other)
		{
			if (other == this)
			{
				return true;
			}
			
			if (other is TermAttribute)
			{
				InitTermBuffer();
				TermAttributeImpl o = ((TermAttributeImpl) other);
				o.InitTermBuffer();
				
				if (termLength != o.termLength)
					return false;
				for (int i = 0; i < termLength; i++)
				{
					if (termBuffer[i] != o.termBuffer[i])
					{
						return false;
					}
				}
				return true;
			}
			
			return false;
		}
		
		public override System.String ToString()
		{
			InitTermBuffer();
			return "term=" + new System.String(termBuffer, 0, termLength);
		}
		
		public override void  CopyTo(AttributeImpl target)
		{
			InitTermBuffer();
			TermAttribute t = (TermAttribute) target;
			t.SetTermBuffer(termBuffer, 0, termLength);
		}
	}
}
