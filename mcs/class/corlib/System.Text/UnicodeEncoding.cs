/*
 * UnicodeEncoding.cs - Implementation of the
 *		"System.Text.UnicodeEncoding" class.
 *
 * Copyright (c) 2001, 2002  Southern Storm Software, Pty Ltd
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

[Serializable]
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
	public UnicodeEncoding () : base(UNICODE_CODE_PAGE)
	{
		bigEndian = false;
		byteOrderMark = true;
	}
	public UnicodeEncoding (bool bigEndian, bool byteOrderMark)
		: base ((bigEndian ? BIG_UNICODE_CODE_PAGE : UNICODE_CODE_PAGE))
	{
		this.bigEndian = bigEndian;
		this.byteOrderMark = byteOrderMark;
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

	// Convenience wrappers for "GetByteCount".
	public override int GetByteCount (String s)
	{
		if (s == null) {
			throw new ArgumentNullException ("s");
		}
		return s.Length * 2;
	}

	// Get the bytes that result from encoding a character buffer.
	public override int GetBytes (char[] chars, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
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
		if ((bytes.Length - byteIndex) < (charCount * 2)) {
			throw new ArgumentException (_("Arg_InsufficientSpace"));
		}
		int posn = byteIndex;
		char ch;
		if (bigEndian) {
			while (charCount-- > 0) {
				ch = chars[charIndex++];
				bytes[posn++] = (byte)(ch >> 8);
				bytes[posn++] = (byte)ch;
			}
		} else {
			while (charCount-- > 0) {
				ch = chars[charIndex++];
				bytes[posn++] = (byte)ch;
				bytes[posn++] = (byte)(ch >> 8);
			}
		}
		return posn - byteIndex;
	}

	// Convenience wrappers for "GetBytes".
	public override int GetBytes (String s, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
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
		if ((bytes.Length - byteIndex) < (charCount * 2)) {
			throw new ArgumentException (_("Arg_InsufficientSpace"));
		}
		int posn = byteIndex;
		char ch;
		if (bigEndian) {
			while (charCount-- > 0) {
				ch = s[charIndex++];
				bytes[posn++] = (byte)(ch >> 8);
				bytes[posn++] = (byte)ch;
			}
		} else {
			while (charCount-- > 0) {
				ch = s[charIndex++];
				bytes[posn++] = (byte)ch;
				bytes[posn++] = (byte)(ch >> 8);
			}
		}
		return posn - byteIndex;
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
		if (count >= 2) {
			if ((bytes[0] == (byte)0xFE && bytes[1] == (byte)0xFF) ||
					(bytes[0] == (byte)0xFF && bytes[1] == (byte)0xFE)) {
				return ((count - 1) / 2);
			}
		}
		return count / 2;
	}

	// Get the characters that result from decoding a byte buffer.
	public override int GetChars (byte[] bytes, int byteIndex, int byteCount,
								 char[] chars, int charIndex)
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

		// Determine the byte order in the incoming buffer.
		bool isBigEndian;
		if (byteCount >= 2) {
			if (bytes[0] == (byte)0xFE && bytes[1] == (byte)0xFF) {
				isBigEndian = true;
				byteCount -= 2;
				byteIndex += 2;
			} else if (bytes[0] == (byte)0xFF && bytes[1] == (byte)0xFE) {
				isBigEndian = false;
				byteCount -= 2;
				byteIndex += 2;
			} else {
				isBigEndian = bigEndian;
			}
		} else {
			isBigEndian = bigEndian;
		}

		// Validate that we have sufficient space in "chars".
		if ((chars.Length - charIndex) < (byteCount / 2)) {
			throw new ArgumentException (_("Arg_InsufficientSpace"));
		}

		// Convert the characters.
		int posn = charIndex;
		if (isBigEndian) {
			while (byteCount >= 2) {
				chars[posn++] =
					((char)((((int)(bytes[byteIndex])) << 8) |
							 ((int)(bytes[byteIndex + 1]))));
				byteIndex += 2;
				byteCount -= 2;
			}
		} else {
			while (byteCount >= 2) {
				chars[posn++] =
					((char)((((int)(bytes[byteIndex + 1])) << 8) |
							 ((int)(bytes[byteIndex]))));
				byteIndex += 2;
				byteCount -= 2;
			}
		}
		return posn - charIndex;
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
		} else {
			return new byte [0];
		}
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

#if !ECMA_COMPAT

	// Get the mail body name for this encoding.
	public override String BodyName
	{
		get {
			if (bigEndian) {
				return "unicodeFFFE";
			} else {
				return "utf-16";
			}
		}
	}

	// Get the human-readable name for this encoding.
	public override String EncodingName
	{
		get {
			if (bigEndian) {
				return "Unicode (Big-Endian)";
			} else {
				return "Unicode";
			}
		}
	}

	// Get the mail agent header name for this encoding.
	public override String HeaderName
	{
		get {
			if (bigEndian) {
				return "unicodeFFFE";
			} else {
				return "utf-16";
			}
		}
	}

	// Determine if this encoding can be saved from a Web browser.
	public override bool IsBrowserSave
	{
		get {
			return !bigEndian;
		}
	}

	// Get the IANA-preferred Web name for this encoding.
	public override String WebName
	{
		get {
			if (bigEndian) {
				// unicodeFFFE is MS compliant, but it is not 
				// valid IANA name.
//				return "unicodeFFFE";
				return "utf-16be";
			} else {
				return "utf-16";
			}
		}
	}

	// Get the Windows code page represented by this object.
	public override int WindowsCodePage
	{
		get {
			// Windows reports the same code page number for
			// both the little-endian and big-endian forms.
			return UNICODE_CODE_PAGE;
		}
	}

#endif // !ECMA_COMPAT

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
		public override int GetChars (byte[] bytes, int byteIndex,
									 int byteCount, char[] chars,
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

			// Convert the characters.
			int posn = charIndex;
			bool isBigEndian = bigEndian;
			int leftOver = leftOverByte;
			int length = chars.Length;
			char ch;
			while (byteCount > 0) {
				if (leftOver != -1) {
					if (isBigEndian) {
						ch = ((char)((leftOver << 8) | ((int)(bytes[byteIndex]))));
					} else {
						ch = ((char)(leftOver |
							    	 (((int)(bytes[byteIndex])) << 8)));
					}
					leftOver = -1;
					++byteIndex;
					--byteCount;
				} else if (byteCount > 1) {
					if (isBigEndian) {
						ch = ((char)((((int)(bytes[byteIndex])) << 8) |
									  ((int)(bytes[byteIndex + 1]))));
					} else {
						ch = ((char)((((int)(bytes[byteIndex + 1])) << 8) |
								      ((int)(bytes[byteIndex]))));
					}
					byteIndex += 2;
					byteCount -= 2;
				} else {
					leftOver = (int)(bytes[byteIndex]);
					break;
				}
				if (ch == '\uFFFE') {
					// Switch byte orders.
					bigEndian = !bigEndian;
				} else if (ch != '\uFEFF') {
					// Ordinary character.
					if (posn < length) {
						chars[posn++] = ch;
					} else {
						throw new ArgumentException (_("Arg_InsufficientSpace"));
					}
				}
			}
			leftOverByte = leftOver;
			bigEndian = isBigEndian;

			// Finished - return the converted length.
			return posn - charIndex;
		}

	} // class UnicodeDecoder

}; // class UnicodeEncoding

}; // namespace System.Text
