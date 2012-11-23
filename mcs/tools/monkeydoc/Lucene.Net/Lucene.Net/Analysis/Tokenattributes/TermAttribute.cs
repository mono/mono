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

using Attribute = Mono.Lucene.Net.Util.Attribute;

namespace Mono.Lucene.Net.Analysis.Tokenattributes
{
	
	/// <summary> The term text of a Token.</summary>
	public interface TermAttribute:Attribute
	{
		/// <summary>Returns the Token's term text.
		/// 
		/// This method has a performance penalty
		/// because the text is stored internally in a char[].  If
		/// possible, use {@link #TermBuffer()} and {@link
		/// #TermLength()} directly instead.  If you really need a
		/// String, use this method, which is nothing more than
		/// a convenience call to <b>new String(token.termBuffer(), 0, token.termLength())</b>
		/// </summary>
		System.String Term();
		
		/// <summary>Copies the contents of buffer, starting at offset for
		/// length characters, into the termBuffer array.
		/// </summary>
		/// <param name="buffer">the buffer to copy
		/// </param>
		/// <param name="offset">the index in the buffer of the first character to copy
		/// </param>
		/// <param name="length">the number of characters to copy
		/// </param>
		void  SetTermBuffer(char[] buffer, int offset, int length);
		
		/// <summary>Copies the contents of buffer into the termBuffer array.</summary>
		/// <param name="buffer">the buffer to copy
		/// </param>
		void  SetTermBuffer(System.String buffer);
		
		/// <summary>Copies the contents of buffer, starting at offset and continuing
		/// for length characters, into the termBuffer array.
		/// </summary>
		/// <param name="buffer">the buffer to copy
		/// </param>
		/// <param name="offset">the index in the buffer of the first character to copy
		/// </param>
		/// <param name="length">the number of characters to copy
		/// </param>
		void  SetTermBuffer(System.String buffer, int offset, int length);
		
		/// <summary>Returns the internal termBuffer character array which
		/// you can then directly alter.  If the array is too
		/// small for your token, use {@link
		/// #ResizeTermBuffer(int)} to increase it.  After
		/// altering the buffer be sure to call {@link
		/// #setTermLength} to record the number of valid
		/// characters that were placed into the termBuffer. 
		/// </summary>
		char[] TermBuffer();
		
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
		char[] ResizeTermBuffer(int newSize);
		
		/// <summary>Return number of valid characters (length of the term)
		/// in the termBuffer array. 
		/// </summary>
		int TermLength();
		
		/// <summary>Set number of valid characters (length of the term) in
		/// the termBuffer array. Use this to truncate the termBuffer
		/// or to synchronize with external manipulation of the termBuffer.
		/// Note: to grow the size of the array,
		/// use {@link #ResizeTermBuffer(int)} first.
		/// </summary>
		/// <param name="length">the truncated length
		/// </param>
		void  SetTermLength(int length);
	}
}
