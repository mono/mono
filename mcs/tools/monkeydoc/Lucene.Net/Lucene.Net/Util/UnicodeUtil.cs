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

namespace Mono.Lucene.Net.Util
{
	
	
	/*
	* Some of this code came from the excellent Unicode
	* conversion examples from:
	*
	*   http://www.unicode.org/Public/PROGRAMS/CVTUTF
	*
	* Full Copyright for that code follows:*/
	
	/*
	* Copyright 2001-2004 Unicode, Inc.
	* 
	* Disclaimer
	* 
	* This source code is provided as is by Unicode, Inc. No claims are
	* made as to fitness for any particular purpose. No warranties of any
	* kind are expressed or implied. The recipient agrees to determine
	* applicability of information provided. If this file has been
	* purchased on magnetic or optical media from Unicode, Inc., the
	* sole remedy for any claim will be exchange of defective media
	* within 90 days of receipt.
	* 
	* Limitations on Rights to Redistribute This Code
	* 
	* Unicode, Inc. hereby grants the right to freely use the information
	* supplied in this file in the creation of products supporting the
	* Unicode Standard, and to make copies of this file in any form
	* for internal or external distribution as long as this notice
	* remains attached.
	*/
	
	/// <summary> Class to encode java's UTF16 char[] into UTF8 byte[]
	/// without always allocating a new byte[] as
	/// String.getBytes("UTF-8") does.
	/// 
	/// <p/><b>WARNING</b>: This API is a new and experimental and
	/// may suddenly change. <p/>
	/// </summary>
	
	sealed public class UnicodeUtil
	{
		
		public const int UNI_SUR_HIGH_START = 0xD800;
		public const int UNI_SUR_HIGH_END = 0xDBFF;
		public const int UNI_SUR_LOW_START = 0xDC00;
		public const int UNI_SUR_LOW_END = 0xDFFF;
		public const int UNI_REPLACEMENT_CHAR = 0xFFFD;
		
		private const long UNI_MAX_BMP = 0x0000FFFF;
		
		private const int HALF_BASE = 0x0010000;
		private const long HALF_SHIFT = 10;
		private const long HALF_MASK = 0x3FFL;
		
		public sealed class UTF8Result
		{
			public byte[] result = new byte[10];
			public int length;
			
			public void  SetLength(int newLength)
			{
				if (result.Length < newLength)
				{
					byte[] newArray = new byte[(int) (1.5 * newLength)];
					Array.Copy(result, 0, newArray, 0, length);
					result = newArray;
				}
				length = newLength;
			}
		}
		
		public sealed class UTF16Result
		{
			public char[] result = new char[10];
			public int[] offsets = new int[10];
			public int length;
			
			public void  SetLength(int newLength)
			{
				if (result.Length < newLength)
				{
					char[] newArray = new char[(int) (1.5 * newLength)];
					Array.Copy(result, 0, newArray, 0, length);
					result = newArray;
				}
				length = newLength;
			}
			
			public void  CopyText(UTF16Result other)
			{
				SetLength(other.length);
				Array.Copy(other.result, 0, result, 0, length);
			}
		}
		
		/// <summary>Encode characters from a char[] source, starting at
		/// offset and stopping when the character 0xffff is seen.
		/// Returns the number of bytes written to bytesOut. 
		/// </summary>
		public static void  UTF16toUTF8(char[] source, int offset, UTF8Result result)
		{
			
			int upto = 0;
			int i = offset;
			byte[] out_Renamed = result.result;
			
			while (true)
			{
				
				int code = (int) source[i++];
				
				if (upto + 4 > out_Renamed.Length)
				{
					byte[] newOut = new byte[2 * out_Renamed.Length];
					System.Diagnostics.Debug.Assert(newOut.Length >= upto + 4);
					Array.Copy(out_Renamed, 0, newOut, 0, upto);
					result.result = out_Renamed = newOut;
				}
				if (code < 0x80)
					out_Renamed[upto++] = (byte) code;
				else if (code < 0x800)
				{
					out_Renamed[upto++] = (byte) (0xC0 | (code >> 6));
					out_Renamed[upto++] = (byte) (0x80 | (code & 0x3F));
				}
				else if (code < 0xD800 || code > 0xDFFF)
				{
					if (code == 0xffff)
					// END
						break;
					out_Renamed[upto++] = (byte) (0xE0 | (code >> 12));
					out_Renamed[upto++] = (byte) (0x80 | ((code >> 6) & 0x3F));
					out_Renamed[upto++] = (byte) (0x80 | (code & 0x3F));
				}
				else
				{
					// surrogate pair
					// confirm valid high surrogate
					if (code < 0xDC00 && source[i] != 0xffff)
					{
						int utf32 = (int) source[i];
						// confirm valid low surrogate and write pair
						if (utf32 >= 0xDC00 && utf32 <= 0xDFFF)
						{
							utf32 = ((code - 0xD7C0) << 10) + (utf32 & 0x3FF);
							i++;
							out_Renamed[upto++] = (byte) (0xF0 | (utf32 >> 18));
							out_Renamed[upto++] = (byte) (0x80 | ((utf32 >> 12) & 0x3F));
							out_Renamed[upto++] = (byte) (0x80 | ((utf32 >> 6) & 0x3F));
							out_Renamed[upto++] = (byte) (0x80 | (utf32 & 0x3F));
							continue;
						}
					}
					// replace unpaired surrogate or out-of-order low surrogate
					// with substitution character
					out_Renamed[upto++] = (byte) (0xEF);
					out_Renamed[upto++] = (byte) (0xBF);
					out_Renamed[upto++] = (byte) (0xBD);
				}
			}
			//assert matches(source, offset, i-offset-1, out, upto);
			result.length = upto;
		}
		
		/// <summary>Encode characters from a char[] source, starting at
		/// offset for length chars.  Returns the number of bytes
		/// written to bytesOut. 
		/// </summary>
		public static void  UTF16toUTF8(char[] source, int offset, int length, UTF8Result result)
		{
			
			int upto = 0;
			int i = offset;
			int end = offset + length;
			byte[] out_Renamed = result.result;
			
			while (i < end)
			{
				
				int code = (int) source[i++];
				
				if (upto + 4 > out_Renamed.Length)
				{
					byte[] newOut = new byte[2 * out_Renamed.Length];
					System.Diagnostics.Debug.Assert(newOut.Length >= upto + 4);
					Array.Copy(out_Renamed, 0, newOut, 0, upto);
					result.result = out_Renamed = newOut;
				}
				if (code < 0x80)
					out_Renamed[upto++] = (byte) code;
				else if (code < 0x800)
				{
					out_Renamed[upto++] = (byte) (0xC0 | (code >> 6));
					out_Renamed[upto++] = (byte) (0x80 | (code & 0x3F));
				}
				else if (code < 0xD800 || code > 0xDFFF)
				{
					out_Renamed[upto++] = (byte) (0xE0 | (code >> 12));
					out_Renamed[upto++] = (byte) (0x80 | ((code >> 6) & 0x3F));
					out_Renamed[upto++] = (byte) (0x80 | (code & 0x3F));
				}
				else
				{
					// surrogate pair
					// confirm valid high surrogate
					if (code < 0xDC00 && i < end && source[i] != 0xffff)
					{
						int utf32 = (int) source[i];
						// confirm valid low surrogate and write pair
						if (utf32 >= 0xDC00 && utf32 <= 0xDFFF)
						{
							utf32 = ((code - 0xD7C0) << 10) + (utf32 & 0x3FF);
							i++;
							out_Renamed[upto++] = (byte) (0xF0 | (utf32 >> 18));
							out_Renamed[upto++] = (byte) (0x80 | ((utf32 >> 12) & 0x3F));
							out_Renamed[upto++] = (byte) (0x80 | ((utf32 >> 6) & 0x3F));
							out_Renamed[upto++] = (byte) (0x80 | (utf32 & 0x3F));
							continue;
						}
					}
					// replace unpaired surrogate or out-of-order low surrogate
					// with substitution character
					out_Renamed[upto++] = (byte) (0xEF);
					out_Renamed[upto++] = (byte) (0xBF);
					out_Renamed[upto++] = (byte) (0xBD);
				}
			}
			//assert matches(source, offset, length, out, upto);
			result.length = upto;
		}
		
		/// <summary>Encode characters from this String, starting at offset
		/// for length characters.  Returns the number of bytes
		/// written to bytesOut. 
		/// </summary>
		public static void  UTF16toUTF8(System.String s, int offset, int length, UTF8Result result)
		{
			int end = offset + length;
			
			byte[] out_Renamed = result.result;
			
			int upto = 0;
			for (int i = offset; i < end; i++)
			{
				int code = (int) s[i];
				
				if (upto + 4 > out_Renamed.Length)
				{
					byte[] newOut = new byte[2 * out_Renamed.Length];
					System.Diagnostics.Debug.Assert(newOut.Length >= upto + 4);
					Array.Copy(out_Renamed, 0, newOut, 0, upto);
					result.result = out_Renamed = newOut;
				}
				if (code < 0x80)
					out_Renamed[upto++] = (byte) code;
				else if (code < 0x800)
				{
					out_Renamed[upto++] = (byte) (0xC0 | (code >> 6));
					out_Renamed[upto++] = (byte) (0x80 | (code & 0x3F));
				}
				else if (code < 0xD800 || code > 0xDFFF)
				{
					out_Renamed[upto++] = (byte) (0xE0 | (code >> 12));
					out_Renamed[upto++] = (byte) (0x80 | ((code >> 6) & 0x3F));
					out_Renamed[upto++] = (byte) (0x80 | (code & 0x3F));
				}
				else
				{
					// surrogate pair
					// confirm valid high surrogate
					if (code < 0xDC00 && (i < end - 1))
					{
						int utf32 = (int) s[i + 1];
						// confirm valid low surrogate and write pair
						if (utf32 >= 0xDC00 && utf32 <= 0xDFFF)
						{
							utf32 = ((code - 0xD7C0) << 10) + (utf32 & 0x3FF);
							i++;
							out_Renamed[upto++] = (byte) (0xF0 | (utf32 >> 18));
							out_Renamed[upto++] = (byte) (0x80 | ((utf32 >> 12) & 0x3F));
							out_Renamed[upto++] = (byte) (0x80 | ((utf32 >> 6) & 0x3F));
							out_Renamed[upto++] = (byte) (0x80 | (utf32 & 0x3F));
							continue;
						}
					}
					// replace unpaired surrogate or out-of-order low surrogate
					// with substitution character
					out_Renamed[upto++] = (byte) (0xEF);
					out_Renamed[upto++] = (byte) (0xBF);
					out_Renamed[upto++] = (byte) (0xBD);
				}
			}
			//assert matches(s, offset, length, out, upto);
			result.length = upto;
		}
		
		/// <summary>Convert UTF8 bytes into UTF16 characters.  If offset
		/// is non-zero, conversion starts at that starting point
		/// in utf8, re-using the results from the previous call
		/// up until offset. 
		/// </summary>
		public static void  UTF8toUTF16(byte[] utf8, int offset, int length, UTF16Result result)
		{
			
			int end = offset + length;
			char[] out_Renamed = result.result;
			if (result.offsets.Length <= end)
			{
				int[] newOffsets = new int[2 * end];
				Array.Copy(result.offsets, 0, newOffsets, 0, result.offsets.Length);
				result.offsets = newOffsets;
			}
			int[] offsets = result.offsets;
			
			// If incremental decoding fell in the middle of a
			// single unicode character, rollback to its start:
			int upto = offset;
			while (offsets[upto] == - 1)
				upto--;
			
			int outUpto = offsets[upto];
			
			// Pre-allocate for worst case 1-for-1
			if (outUpto + length >= out_Renamed.Length)
			{
				char[] newOut = new char[2 * (outUpto + length)];
				Array.Copy(out_Renamed, 0, newOut, 0, outUpto);
				result.result = out_Renamed = newOut;
			}
			
			while (upto < end)
			{
				
				int b = utf8[upto] & 0xff;
				int ch;
				
				offsets[upto++] = outUpto;
				
				if (b < 0xc0)
				{
					System.Diagnostics.Debug.Assert(b < 0x80);
					ch = b;
				}
				else if (b < 0xe0)
				{
					ch = ((b & 0x1f) << 6) + (utf8[upto] & 0x3f);
					offsets[upto++] = - 1;
				}
				else if (b < 0xf0)
				{
					ch = ((b & 0xf) << 12) + ((utf8[upto] & 0x3f) << 6) + (utf8[upto + 1] & 0x3f);
					offsets[upto++] = - 1;
					offsets[upto++] = - 1;
				}
				else
				{
					System.Diagnostics.Debug.Assert(b < 0xf8);
					ch = ((b & 0x7) << 18) + ((utf8[upto] & 0x3f) << 12) + ((utf8[upto + 1] & 0x3f) << 6) + (utf8[upto + 2] & 0x3f);
					offsets[upto++] = - 1;
					offsets[upto++] = - 1;
					offsets[upto++] = - 1;
				}
				
				if (ch <= UNI_MAX_BMP)
				{
					// target is a character <= 0xFFFF
					out_Renamed[outUpto++] = (char) ch;
				}
				else
				{
					// target is a character in range 0xFFFF - 0x10FFFF
					int chHalf = ch - HALF_BASE;
					out_Renamed[outUpto++] = (char) ((chHalf >> (int) HALF_SHIFT) + UNI_SUR_HIGH_START);
					out_Renamed[outUpto++] = (char) ((chHalf & HALF_MASK) + UNI_SUR_LOW_START);
				}
			}
			
			offsets[upto] = outUpto;
			result.length = outUpto;
		}
		
		// Only called from assert
		/*
		private static boolean matches(char[] source, int offset, int length, byte[] result, int upto) {
		try {
		String s1 = new String(source, offset, length);
		String s2 = new String(result, 0, upto, "UTF-8");
		if (!s1.equals(s2)) {
		//System.out.println("DIFF: s1 len=" + s1.length());
		//for(int i=0;i<s1.length();i++)
		//  System.out.println("    " + i + ": " + (int) s1.charAt(i));
		//System.out.println("s2 len=" + s2.length());
		//for(int i=0;i<s2.length();i++)
		//  System.out.println("    " + i + ": " + (int) s2.charAt(i));
		
		// If the input string was invalid, then the
		// difference is OK
		if (!validUTF16String(s1))
		return true;
		
		return false;
		}
		return s1.equals(s2);
		} catch (UnsupportedEncodingException uee) {
		return false;
		}
		}
		
		// Only called from assert
		private static boolean matches(String source, int offset, int length, byte[] result, int upto) {
		try {
		String s1 = source.substring(offset, offset+length);
		String s2 = new String(result, 0, upto, "UTF-8");
		if (!s1.equals(s2)) {
		// Allow a difference if s1 is not valid UTF-16
		
		//System.out.println("DIFF: s1 len=" + s1.length());
		//for(int i=0;i<s1.length();i++)
		//  System.out.println("    " + i + ": " + (int) s1.charAt(i));
		//System.out.println("  s2 len=" + s2.length());
		//for(int i=0;i<s2.length();i++)
		//  System.out.println("    " + i + ": " + (int) s2.charAt(i));
		
		// If the input string was invalid, then the
		// difference is OK
		if (!validUTF16String(s1))
		return true;
		
		return false;
		}
		return s1.equals(s2);
		} catch (UnsupportedEncodingException uee) {
		return false;
		}
		}
		
		public static final boolean validUTF16String(String s) {
		final int size = s.length();
		for(int i=0;i<size;i++) {
		char ch = s.charAt(i);
		if (ch >= UNI_SUR_HIGH_START && ch <= UNI_SUR_HIGH_END) {
		if (i < size-1) {
		i++;
		char nextCH = s.charAt(i);
		if (nextCH >= UNI_SUR_LOW_START && nextCH <= UNI_SUR_LOW_END) {
		// Valid surrogate pair
		} else
		// Unmatched hight surrogate
		return false;
		} else
		// Unmatched hight surrogate
		return false;
		} else if (ch >= UNI_SUR_LOW_START && ch <= UNI_SUR_LOW_END)
		// Unmatched low surrogate
		return false;
		}
		
		return true;
		}
		
		public static final boolean validUTF16String(char[] s, int size) {
		for(int i=0;i<size;i++) {
		char ch = s[i];
		if (ch >= UNI_SUR_HIGH_START && ch <= UNI_SUR_HIGH_END) {
		if (i < size-1) {
		i++;
		char nextCH = s[i];
		if (nextCH >= UNI_SUR_LOW_START && nextCH <= UNI_SUR_LOW_END) {
		// Valid surrogate pair
		} else
		return false;
		} else
		return false;
		} else if (ch >= UNI_SUR_LOW_START && ch <= UNI_SUR_LOW_END)
		// Unmatched low surrogate
		return false;
		}
		
		return true;
		}
		*/
	}
}
