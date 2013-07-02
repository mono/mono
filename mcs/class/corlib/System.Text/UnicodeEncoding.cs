/*
 * UnicodeEncoding.cs - Implementation of the
 *		"System.Text.UnicodeEncoding" class.
 *
 * Copyright (c) 2001, 2002  Southern Storm Software, Pty Ltd
 * Copyright (C) 2003, 2004 Novell, Inc.
 * Copyright (C) 2006 Kornél Pál <http://www.kornelpal.hu/>
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

namespace System.Text
{

using System;
using System.Runtime.InteropServices;

[Serializable]
[ComVisible (true)]
[MonoLimitation ("Serialization format not compatible with .NET")]
public class UnicodeEncoding : Encoding
{
	// Magic numbers used by Windows for Unicode.
	internal const int UNICODE_CODE_PAGE     = 1200;
	internal const int BIG_UNICODE_CODE_PAGE = 1201;

#if !ECMA_COMPAT
	// Size of characters in this encoding.
	public const int CharSize = 2;
#endif

	// Internal state.
	private bool bigEndian;
	private bool byteOrderMark;

	// Constructors.
	public UnicodeEncoding () : this (false, true)
	{
		bigEndian = false;
		byteOrderMark = true;
	}
	public UnicodeEncoding (bool bigEndian, bool byteOrderMark)
		: this (bigEndian, byteOrderMark, false)
	{
	}

	public UnicodeEncoding (bool bigEndian, bool byteOrderMark, bool throwOnInvalidBytes)
		: base ((bigEndian ? BIG_UNICODE_CODE_PAGE : UNICODE_CODE_PAGE))
	{
		if (throwOnInvalidBytes)
			SetFallbackInternal (null, new DecoderExceptionFallback ());
		else
			SetFallbackInternal (null, new DecoderReplacementFallback ("\uFFFD"));

		this.bigEndian = bigEndian;
		this.byteOrderMark = byteOrderMark;

		if (bigEndian){
			body_name = "unicodeFFFE";
			encoding_name = "Unicode (Big-Endian)";
			header_name = "unicodeFFFE";
			is_browser_save = false;
			web_name = "unicodeFFFE";
		} else {
			body_name = "utf-16";
			encoding_name = "Unicode";
			header_name = "utf-16";
			is_browser_save = true;
			web_name = "utf-16";
		}
		
		// Windows reports the same code page number for
		// both the little-endian and big-endian forms.
		windows_code_page = UNICODE_CODE_PAGE;
	}

	// Get the number of bytes needed to encode a character buffer.
	public override int GetByteCount (char[] chars, int index, int count)
	{
		if (chars == null) {
			throw new ArgumentNullException ("chars");
		}
		if (index < 0 || index > chars.Length) {
			throw new ArgumentOutOfRangeException ("index", _("ArgRange_Array"));
		}
		if (count < 0 || count > (chars.Length - index)) {
			throw new ArgumentOutOfRangeException ("count", _("ArgRange_Array"));
		}
		return count * 2;
	}

	public override int GetByteCount (String s)
	{
		if (s == null) {
			throw new ArgumentNullException ("s");
		}
		return s.Length * 2;
	}

	[CLSCompliantAttribute (false)]
	[ComVisible (false)]
	public unsafe override int GetByteCount (char* chars, int count)
	{
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (count < 0)
			throw new ArgumentOutOfRangeException ("count");

		return count * 2;
	}

	// Get the bytes that result from encoding a character buffer.
	public unsafe override int GetBytes (char [] chars, int charIndex, int charCount,
										byte [] bytes, int byteIndex)
	{
		if (chars == null) {
			throw new ArgumentNullException ("chars");
		}
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		if (charIndex < 0 || charIndex > chars.Length) {
			throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_Array"));
		}
		if (charCount < 0 || charCount > (chars.Length - charIndex)) {
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_Array"));
		}
		if (byteIndex < 0 || byteIndex > bytes.Length) {
			throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));
		}

		if (charCount == 0)
			return 0;

		int byteCount = bytes.Length - byteIndex;
		if (bytes.Length == 0)
			bytes = new byte [1];

		fixed (char* charPtr = chars)
			fixed (byte* bytePtr = bytes)
				return GetBytesInternal (charPtr + charIndex, charCount, bytePtr + byteIndex, byteCount);
	}

	public unsafe override int GetBytes (String s, int charIndex, int charCount,
										byte [] bytes, int byteIndex)
	{
		if (s == null) {
			throw new ArgumentNullException ("s");
		}
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		if (charIndex < 0 || charIndex > s.Length) {
			throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_StringIndex"));
		}
		if (charCount < 0 || charCount > (s.Length - charIndex)) {
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_StringRange"));
		}
		if (byteIndex < 0 || byteIndex > bytes.Length) {
			throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));
		}

		// For consistency
		if (charCount == 0)
			return 0;

		int byteCount = bytes.Length - byteIndex;
		if (bytes.Length == 0)
			bytes = new byte [1];

		fixed (char* charPtr = s)
			fixed (byte* bytePtr = bytes)
				return GetBytesInternal (charPtr + charIndex, charCount, bytePtr + byteIndex, byteCount);
	}

	[CLSCompliantAttribute (false)]
	[ComVisible (false)]
	public unsafe override int GetBytes (char* chars, int charCount,
										byte* bytes, int byteCount)
	{
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount");
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount");

		return GetBytesInternal (chars, charCount, bytes, byteCount);
	}

	private unsafe int GetBytesInternal (char* chars, int charCount,
										byte* bytes, int byteCount)
	{
		int count = charCount * 2;

		if (byteCount < count)
			throw new ArgumentException (_("Arg_InsufficientSpace"));

		CopyChars ((byte*) chars, bytes, count, bigEndian);
		return count;
	}

	// Get the number of characters needed to decode a byte buffer.
	public override int GetCharCount (byte[] bytes, int index, int count)
	{
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		if (index < 0 || index > bytes.Length) {
			throw new ArgumentOutOfRangeException ("index", _("ArgRange_Array"));
		}
		if (count < 0 || count > (bytes.Length - index)) {
			throw new ArgumentOutOfRangeException ("count", _("ArgRange_Array"));
		}
		return count / 2;
	}

	[CLSCompliantAttribute (false)]
	[ComVisible (false)]
	public unsafe override int GetCharCount (byte* bytes, int count)
	{
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (count < 0)
			throw new ArgumentOutOfRangeException ("count");

		return count / 2;
	}

	// Get the characters that result from decoding a byte buffer.
	public unsafe override int GetChars (byte [] bytes, int byteIndex, int byteCount,
										char [] chars, int charIndex)
	{
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		if (chars == null) {
			throw new ArgumentNullException ("chars");
		}
		if (byteIndex < 0 || byteIndex > bytes.Length) {
			throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));
		}
		if (byteCount < 0 || byteCount > (bytes.Length - byteIndex)) {
			throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_Array"));
		}
		if (charIndex < 0 || charIndex > chars.Length) {
			throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_Array"));
		}

		if (byteCount == 0)
			return 0;

		int charCount = chars.Length - charIndex;
		if (chars.Length == 0)
			chars = new char [1];

		fixed (byte* bytePtr = bytes)
			fixed (char* charPtr = chars)
				return GetCharsInternal (bytePtr + byteIndex, byteCount, charPtr + charIndex, charCount);
}

	[CLSCompliantAttribute (false)]
	[ComVisible (false)]
	public unsafe override int GetChars (byte* bytes, int byteCount,
										char* chars, int charCount)
	{
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount");
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount");

		return GetCharsInternal (bytes, byteCount, chars, charCount);
	}

	// Decode a buffer of bytes into a string.
	[ComVisible (false)]
	public unsafe override String GetString (byte [] bytes, int index, int count)
	{
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (index < 0 || index > bytes.Length)
			throw new ArgumentOutOfRangeException ("index", _("ArgRange_Array"));
		if (count < 0 || count > (bytes.Length - index))
			throw new ArgumentOutOfRangeException ("count", _("ArgRange_Array"));

		if (count == 0)
			return string.Empty;

		// GetCharCountInternal
		int charCount = count / 2;
		string s = string.InternalAllocateStr (charCount);

		fixed (byte* bytePtr = bytes)
			fixed (char* charPtr = s)
				GetCharsInternal (bytePtr + index, count, charPtr, charCount);

		return s;
	}

	private unsafe int GetCharsInternal (byte* bytes, int byteCount,
										char* chars, int charCount)
	{
		int count = byteCount / 2;

		// Validate that we have sufficient space in "chars".
		if (charCount < count)
			throw new ArgumentException (_("Arg_InsufficientSpace"));

		CopyChars (bytes, (byte*) chars, byteCount, bigEndian);
		return count;
	}

	[ComVisible (false)]
	public override Encoder GetEncoder ()
	{
		return(base.GetEncoder ());
	}
	
	// Get the maximum number of bytes needed to encode a
	// specified number of characters.
	public override int GetMaxByteCount (int charCount)
	{
		if (charCount < 0) {
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_NonNegative"));
		}
		return charCount * 2;
	}

	// Get the maximum number of characters needed to decode a
	// specified number of bytes.
	public override int GetMaxCharCount (int byteCount)
	{
		if (byteCount < 0) {
			throw new ArgumentOutOfRangeException
				("byteCount", _("ArgRange_NonNegative"));
		}
		return byteCount / 2;
	}

	// Get a Unicode-specific decoder that is attached to this instance.
	public override Decoder GetDecoder ()
	{
		return new UnicodeDecoder (bigEndian);
	}

	// Get the Unicode preamble.
	public override byte[] GetPreamble ()
	{
		if (byteOrderMark) {
			byte[] preamble = new byte[2];
			if (bigEndian) {
				preamble[0] = (byte)0xFE;
				preamble[1] = (byte)0xFF;
			} else {
				preamble[0] = (byte)0xFF;
				preamble[1] = (byte)0xFE;
			}
			return preamble;
		}
		
		return EmptyArray<byte>.Value;
	}

	// Determine if this object is equal to another.
	public override bool Equals (Object value)
	{
		UnicodeEncoding enc = (value as UnicodeEncoding);
		if (enc != null) {
			return (codePage == enc.codePage &&
					bigEndian == enc.bigEndian &&
					byteOrderMark == enc.byteOrderMark);
		} else {
			return false;
		}
	}

	// Get the hash code for this object.
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}

	private unsafe static void CopyChars (byte* src, byte* dest, int count, bool bigEndian)
	{
		if (BitConverter.IsLittleEndian != bigEndian) {
			string.memcpy (dest, src, count & unchecked ((int) 0xFFFFFFFE));
			return;
		}

		switch (count) {
		case 0:
			return;
		case 1:
			return;
		case 2:
			goto Count2;
		case 3:
			goto Count2;
		case 4:
			goto Count4;
		case 5:
			goto Count4;
		case 6:
			goto Count4;
		case 7:
			goto Count4;
		case 8:
			goto Count8;
		case 9:
			goto Count8;
		case 10:
			goto Count8;
		case 11:
			goto Count8;
		case 12:
			goto Count8;
		case 13:
			goto Count8;
		case 14:
			goto Count8;
		case 15:
			goto Count8;
		}

		do {
			dest [0] = src [1];
			dest [1] = src [0];
			dest [2] = src [3];
			dest [3] = src [2];
			dest [4] = src [5];
			dest [5] = src [4];
			dest [6] = src [7];
			dest [7] = src [6];
			dest [8] = src [9];
			dest [9] = src [8];
			dest [10] = src [11];
			dest [11] = src [10];
			dest [12] = src [13];
			dest [13] = src [12];
			dest [14] = src [15];
			dest [15] = src [14];
			dest += 16;
			src += 16;
			count -= 16;
		} while ((count & unchecked ((int) 0xFFFFFFF0)) != 0);

		switch (count) {
		case 0:
			return;
		case 1:
			return;
		case 2:
			goto Count2;
		case 3:
			goto Count2;
		case 4:
			goto Count4;
		case 5:
			goto Count4;
		case 6:
			goto Count4;
		case 7:
			goto Count4;
		}

		Count8:;
		dest [0] = src [1];
		dest [1] = src [0];
		dest [2] = src [3];
		dest [3] = src [2];
		dest [4] = src [5];
		dest [5] = src [4];
		dest [6] = src [7];
		dest [7] = src [6];
		dest += 8;
		src += 8;

		if ((count & 4) == 0)
			goto TestCount2;
		Count4:;
		dest [0] = src [1];
		dest [1] = src [0];
		dest [2] = src [3];
		dest [3] = src [2];
		dest += 4;
		src += 4;

		TestCount2:;
		if ((count & 2) == 0)
			return;
		Count2:;
		dest [0] = src [1];
		dest [1] = src [0];
	}

	// Unicode decoder implementation.
	private sealed class UnicodeDecoder : Decoder
	{
		private bool bigEndian;
		private int leftOverByte;

		// Constructor.
		public UnicodeDecoder (bool bigEndian)
		{
			this.bigEndian = bigEndian;
			leftOverByte = -1;
		}

		// Override inherited methods.
		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			if (bytes == null) {
				throw new ArgumentNullException ("bytes");
			}
			if (index < 0 || index > bytes.Length) {
				throw new ArgumentOutOfRangeException ("index", _("ArgRange_Array"));
			}
			if (count < 0 || count > (bytes.Length - index)) {
				throw new ArgumentOutOfRangeException ("count", _("ArgRange_Array"));
			}
			if (leftOverByte != -1) {
				return (count + 1) / 2;
			} else {
				return count / 2;
			}
		}
		
		public unsafe override int GetChars (byte [] bytes, int byteIndex,
											int byteCount, char [] chars,
											int charIndex)
		{
			if (bytes == null) {
				throw new ArgumentNullException ("bytes");
			}
			if (chars == null) {
				throw new ArgumentNullException ("chars");
			}
			if (byteIndex < 0 || byteIndex > bytes.Length) {
				throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));
			}
			if (byteCount < 0 || byteCount > (bytes.Length - byteIndex)) {
				throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_Array"));
			}
			if (charIndex < 0 || charIndex > chars.Length) {
				throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_Array"));
			}

			if (byteCount == 0)
				return 0;

			int leftOver = leftOverByte;
			int count;

			if (leftOver != -1)
				count = (byteCount + 1) / 2;
			else
				count = byteCount / 2;

			if (chars.Length - charIndex < count)
				throw new ArgumentException (_("Arg_InsufficientSpace"));

			if (leftOver != -1) {
				if (bigEndian)
					chars [charIndex] = unchecked ((char) ((leftOver << 8) | (int) bytes [byteIndex]));
				else
					chars [charIndex] = unchecked ((char) (((int) bytes [byteIndex] << 8) | leftOver));
				charIndex++;
				byteIndex++;
				byteCount--;
			}

			if ((byteCount & unchecked ((int) 0xFFFFFFFE)) != 0)
				fixed (byte* bytePtr = bytes)
					fixed (char* charPtr = chars)
						CopyChars (bytePtr + byteIndex, (byte*) (charPtr + charIndex), byteCount, bigEndian);

			if ((byteCount & 1) == 0)
				leftOverByte = -1;
			else
				leftOverByte = bytes [byteCount + byteIndex - 1];

			return count;
		}

	} // class UnicodeDecoder

}; // class UnicodeEncoding

}; // namespace System.Text
