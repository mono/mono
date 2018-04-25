/*
 * CP51932.cs - Japanese EUC-JP code page.
 *
 * It is based on CP932.cs from Portable.NET
 *
 * Author:
 *	Atsushi Enomoto <atsushi@ximian.com>
 *
 * Below are original (CP932.cs) copyright lines
 *
 * (C)2004 Novell Inc.
 *
 * Copyright (c) 2002  Southern Storm Software, Pty Ltd
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

/*

	jis.table is generated from uni2tab.c, which requires CP932.TXT and
	Unihan.txt as input files. However, it is non-normative and in Japan it is
	contains many problem.

	FIXME:  Some characters such as 0xFF0B (wide "plus") are missing in
		that table.
*/

/*
	0x00-0x1F, 0x7F   : control characters
	0x20-0x7E         : ASCII
	0xA1A1-0xFEFE     : Kanji (precisely, both bytes contain only A1-FE)
	0x8EA1-0x8EDF     : half-width Katakana
	0x8FA1A1-0x8FFEFE : Complemental Kanji

*/

namespace I18N.CJK
{

using System;
using System.Text;
using I18N.Common;

#if DISABLE_UNSAFE
using MonoEncoder = I18N.Common.MonoSafeEncoder;
using MonoEncoding = I18N.Common.MonoSafeEncoding;
#endif

[Serializable]
public class CP51932 : MonoEncoding
{
	// Magic number used by Windows for the EUC-JP code page.
	private const int EUC_JP_CODE_PAGE = 51932;

	// Constructor.
	public CP51932 () : base (EUC_JP_CODE_PAGE, 932)
	{
	}

#if !DISABLE_UNSAFE
	public unsafe override int GetByteCountImpl (char* chars, int count)
	{
		return new CP51932Encoder (this).GetByteCountImpl (chars, count, true);
	}

	public unsafe override int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount)
	{
		return new CP51932Encoder (this).GetBytesImpl (chars, charCount, bytes, byteCount, true);
	}
#else
	public override int GetByteCount (char [] chars, int index, int length)
	{
		return new CP51932Encoder (this).GetByteCount (chars, index, length, true);
	}

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		return new CP51932Encoder (this).GetBytes (chars, charIndex, charCount, bytes, byteIndex, true);
	}
#endif

	public override int GetCharCount (byte [] bytes, int index, int count)
	{
		return new CP51932Decoder ().GetCharCount (
			bytes, index, count, true);
	}

	public override int GetChars (
		byte [] bytes, int byteIndex, int byteCount,
		char [] chars, int charIndex)
	{
		return new CP51932Decoder ().GetChars (bytes,
			byteIndex, byteCount, chars, charIndex, true);
	}

	// Get the maximum number of bytes needed to encode a
	// specified number of characters.
	public override int GetMaxByteCount(int charCount)
	{
		if(charCount < 0)
		{
			throw new ArgumentOutOfRangeException
				("charCount",
				 Strings.GetString("ArgRange_NonNegative"));
		}
		return charCount * 3;
	}

	// Get the maximum number of characters needed to decode a
	// specified number of bytes.
	public override int GetMaxCharCount(int byteCount)
	{
		if(byteCount < 0)
		{
			throw new ArgumentOutOfRangeException
				("byteCount",
				 Strings.GetString ("ArgRange_NonNegative"));
		}
		return byteCount;
	}

	public override Encoder GetEncoder ()
	{
		return new CP51932Encoder (this);
	}

	public override Decoder GetDecoder ()
	{
		return new CP51932Decoder ();
	}

#if !ECMA_COMPAT

	// Get the mail body name for this encoding.
	public override String BodyName {
		get { return "euc-jp"; }
	}

	// Get the human-readable name for this encoding.
	public override String EncodingName {
		get { return "Japanese (EUC)"; }
	}

	// Get the mail agent header name for this encoding.
	public override String HeaderName {
		get { return "euc-jp"; }
	}

	// Determine if this encoding can be displayed in a Web browser.
	public override bool IsBrowserDisplay {
		get { return true; }
	}

	// Determine if this encoding can be saved from a Web browser.
	public override bool IsBrowserSave {
		get { return true; }
	}

	// Determine if this encoding can be displayed in a mail/news agent.
	public override bool IsMailNewsDisplay {
		get { return true; }
	}

	// Determine if this encoding can be saved from a mail/news agent.
	public override bool IsMailNewsSave {
		get { return true; }
	}

	// Get the IANA-preferred Web name for this encoding.
	public override String WebName {
		get { return "euc-jp"; }
	}
} // CP51932
#endif // !ECMA_COMPAT

public class CP51932Encoder : MonoEncoder
{
	public CP51932Encoder (MonoEncoding encoding)
		: base (encoding)
	{
	}

#if !DISABLE_UNSAFE
	// Get the number of bytes needed to encode a character buffer.
	public unsafe override int GetByteCountImpl (
		char* chars, int count, bool refresh)
	{
		// Determine the length of the final output.
		int index = 0;
		int length = 0;
		int ch, value;
		byte [] cjkToJis = JISConvert.Convert.cjkToJis;
		byte [] extraToJis = JISConvert.Convert.extraToJis;

		while (count > 0) {
			ch = chars [index++];
			--count;
			++length;
			if (ch < 0x0080) {
				// Character maps to itself.
				continue;
			} else if (ch < 0x0100) {
				// Check for special Latin 1 characters that
				// can be mapped to double-byte code points.
				if(ch == 0x00A2 || ch == 0x00A3 || ch == 0x00A7 ||
				   ch == 0x00A8 || ch == 0x00AC || ch == 0x00B0 ||
				   ch == 0x00B1 || ch == 0x00B4 || ch == 0x00B6 ||
				   ch == 0x00D7 || ch == 0x00F7)
				{
					++length;
				}
			} else if (ch >= 0x0391 && ch <= 0x0451) {
				// Greek subset characters.
				++length;
			} else if (ch >= 0x2010 && ch <= 0x9FA5) {
				// This range contains the bulk of the CJK set.
				value = (ch - 0x2010) * 2;
				value = ((int) (cjkToJis[value])) | (((int)(cjkToJis[value + 1])) << 8);
				if(value >= 0x0100)
					++length;
			} else if(ch >= 0xFF01 && ch < 0xFF60) {
				// This range contains extra characters.
				value = (ch - 0xFF01) * 2;
				value = ((int)(extraToJis[value])) |
						(((int)(extraToJis[value + 1])) << 8);
				if(value >= 0x0100)
					++length;
			} else if(ch >= 0xFF60 && ch <= 0xFFA0) {
				++length; // half-width kana
			}
		}

		// Return the length to the caller.
		return length;
	}

	// Get the bytes that result from encoding a character buffer.
	public unsafe override int GetBytesImpl (
		char* chars, int charCount, byte* bytes, int byteCount, bool refresh)
	{
		int charIndex = 0;
		int byteIndex = 0;
		int end = charCount;

		// Convert the characters into their byte form.
		int posn = byteIndex;
		int byteLength = byteCount;
		int ch, value;

		byte[] cjkToJis = JISConvert.Convert.cjkToJis;
		byte[] greekToJis = JISConvert.Convert.greekToJis;
		byte[] extraToJis = JISConvert.Convert.extraToJis;

		for (int i = charIndex; i < end; i++, charCount--) {
			ch = chars [i];
			if (posn >= byteLength) {
				throw new ArgumentException (Strings.GetString ("Arg_InsufficientSpace"), "bytes");
			}

			if (ch < 0x0080) {
				// Character maps to itself.
				bytes[posn++] = (byte)ch;
				continue;
			} else if (ch >= 0x0391 && ch <= 0x0451) {
				// Greek subset characters.
				value = (ch - 0x0391) * 2;
				value = ((int)(greekToJis[value])) |
						(((int)(greekToJis[value + 1])) << 8);
			} else if (ch >= 0x2010 && ch <= 0x9FA5) {
				// This range contains the bulk of the CJK set.
				value = (ch - 0x2010) * 2;
				value = ((int) (cjkToJis[value])) |
						(((int)(cjkToJis[value + 1])) << 8);
			} else if (ch >= 0xFF01 && ch <= 0xFF60) {
				// This range contains extra characters,
				// including half-width katakana.
				value = (ch - 0xFF01) * 2;
				value = ((int) (extraToJis [value])) |
						(((int) (extraToJis [value + 1])) << 8);
			} else if (ch >= 0xFF60 && ch <= 0xFFA0) {
				value = ch - 0xFF60 + 0x8EA0;
			} else {
				// Invalid character.
				value = 0;
			}

			if (value == 0) {
				HandleFallback (
					chars, ref i, ref charCount,
					bytes, ref posn, ref byteCount, null);
			} else if (value < 0x0100) {
				bytes [posn++] = (byte) value;
			} else if ((posn + 1) >= byteLength) {
				throw new ArgumentException (Strings.GetString ("Arg_InsufficientSpace"), "bytes");
			} else if (value < 0x8000) {
				// general 2byte glyph/kanji
				value -= 0x0100;
				bytes [posn++] = (byte) (value / 0x5E + 0xA1);
				bytes [posn++] = (byte) (value % 0x5E + 0xA1);
//Console.WriteLine ("{0:X04}", ch);
				continue;
			}
			else
			{
				// half-width kana
				bytes [posn++] = 0x8E;
				bytes [posn++] = (byte) (value - 0x8E00);
			}
		}

		// Return the final length to the caller.
		return posn - byteIndex;
	}
#else
	// Get the number of bytes needed to encode a character buffer.
	public override int GetByteCount(char[] chars, int index, int count, bool flush)
	{
		// Determine the length of the final output.
		int length = 0;
		int ch, value;
		byte[] cjkToJis = JISConvert.Convert.cjkToJis;
		byte[] extraToJis = JISConvert.Convert.extraToJis;

		while (count > 0)
		{
			ch = chars[index++];
			--count;
			++length;
			if (ch < 0x0080)
			{
				// Character maps to itself.
				continue;
			}
			else if (ch < 0x0100)
			{
				// Check for special Latin 1 characters that
				// can be mapped to double-byte code points.
				if (ch == 0x00A2 || ch == 0x00A3 || ch == 0x00A7 ||
				   ch == 0x00A8 || ch == 0x00AC || ch == 0x00B0 ||
				   ch == 0x00B1 || ch == 0x00B4 || ch == 0x00B6 ||
				   ch == 0x00D7 || ch == 0x00F7)
				{
					++length;
				}
			}
			else if (ch >= 0x0391 && ch <= 0x0451)
			{
				// Greek subset characters.
				++length;
			}
			else if (ch >= 0x2010 && ch <= 0x9FA5)
			{
				// This range contains the bulk of the CJK set.
				value = (ch - 0x2010) * 2;
				value = ((int)(cjkToJis[value])) | (((int)(cjkToJis[value + 1])) << 8);
				if (value >= 0x0100)
					++length;
			}
			else if (ch >= 0xFF01 && ch < 0xFF60)
			{
				// This range contains extra characters.
				value = (ch - 0xFF01) * 2;
				value = ((int)(extraToJis[value])) |
						(((int)(extraToJis[value + 1])) << 8);
				if (value >= 0x0100)
					++length;
			}
			else if (ch >= 0xFF60 && ch <= 0xFFA0)
			{
				++length; // half-width kana
			}
		}

		// Return the length to the caller.
		return length;
	}

	// Get the bytes that result from encoding a character buffer.
	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
	{
		// Convert the characters into their byte form.
		int posn = byteIndex;
		int byteLength = bytes.Length;
		int byteCount = bytes.Length;
		int end = charIndex + charCount;
		int ch, value;

		byte[] cjkToJis = JISConvert.Convert.cjkToJis;
		byte[] greekToJis = JISConvert.Convert.greekToJis;
		byte[] extraToJis = JISConvert.Convert.extraToJis;

		for (int i = charIndex; i < end; i++, charCount--)
		{
			ch = chars[i];
			if (posn >= byteLength)
			{
				throw new ArgumentException(Strings.GetString("Arg_InsufficientSpace"), "bytes");
			}

			if (ch < 0x0080)
			{
				// Character maps to itself.
				bytes[posn++] = (byte)ch;
				continue;
			}
			else if (ch >= 0x0391 && ch <= 0x0451)
			{
				// Greek subset characters.
				value = (ch - 0x0391) * 2;
				value = ((int)(greekToJis[value])) |
						(((int)(greekToJis[value + 1])) << 8);
			}
			else if (ch >= 0x2010 && ch <= 0x9FA5)
			{
				// This range contains the bulk of the CJK set.
				value = (ch - 0x2010) * 2;
				value = ((int)(cjkToJis[value])) |
						(((int)(cjkToJis[value + 1])) << 8);
			}
			else if (ch >= 0xFF01 && ch <= 0xFF60)
			{
				// This range contains extra characters,
				// including half-width katakana.
				value = (ch - 0xFF01) * 2;
				value = ((int)(extraToJis[value])) |
						(((int)(extraToJis[value + 1])) << 8);
			}
			else if (ch >= 0xFF60 && ch <= 0xFFA0)
			{
				value = ch - 0xFF60 + 0x8EA0;
			}
			else
			{
				// Invalid character.
				value = 0;
			}

			if (value == 0)
			{
				HandleFallback (chars, ref i, ref charCount,
					bytes, ref posn, ref byteCount, null);
			}
			else if (value < 0x0100)
			{
				bytes[posn++] = (byte)value;
			}
			else if ((posn + 1) >= byteLength)
			{
				throw new ArgumentException(Strings.GetString("Arg_InsufficientSpace"), "bytes");
			}
			else if (value < 0x8000)
			{
				// general 2byte glyph/kanji
				value -= 0x0100;
				bytes[posn++] = (byte)(value / 0x5E + 0xA1);
				bytes[posn++] = (byte)(value % 0x5E + 0xA1);
				//Console.WriteLine ("{0:X04}", ch);
				continue;
			}
			else
			{
				// half-width kana
				bytes[posn++] = 0x8E;
				bytes[posn++] = (byte)(value - 0x8E00);
			}
		}

		// Return the final length to the caller.
		return posn - byteIndex;
	}
#endif
} // CP51932Encoder

internal class CP51932Decoder : DbcsEncoding.DbcsDecoder
{
	public CP51932Decoder ()
		: base (null)
	{
	}

	int last_count, last_bytes;

	// Get the number of characters needed to decode a byte buffer.
	public override int GetCharCount (byte [] bytes, int index, int count)
	{
		return GetCharCount (bytes, index, count, false);
	}

	public override
	int GetCharCount (byte [] bytes, int index, int count, bool refresh)
	{
		CheckRange (bytes, index, count);

		// Determine the total length of the converted string.
		int value = 0;
		byte[] table0208 = JISConvert.Convert.jisx0208ToUnicode;
		byte[] table0212 = JISConvert.Convert.jisx0212ToUnicode;
		int length = 0;
		int byteval = 0;
		int last = last_count;

		while (count > 0) {
			byteval = bytes [index++];
			--count;
			if (last == 0) {
				if (byteval == 0x8F) {
					// SS3: One-time triple-byte sequence should follow.
					last = byteval;
				} else if (byteval <= 0x7F) {
					// Ordinary ASCII/Latin1/Control character.
					length++;
				} else if (byteval == 0x8E) {
					// SS2: One-time double-byte sequence should follow.
					last = byteval;
				} else if (byteval >= 0xA1 && byteval <= 0xFE) {
					// First byte in a double-byte sequence.
					last = byteval;
				} else {
					// Invalid first byte.
					length++;
				}
			}
			else if (last == 0x8E) {
				// SS2 (One-time double-byte sequence)
				if (byteval >= 0xA1 && byteval <= 0xDF) {
					length++;
				} else {
					// Invalid second byte.
					length++;
				}
				last =0;
			}
			else if (last == 0x8F) {
				// SS3: 3-byte character
				// FIXME: not supported (I don't think iso-2022-jp has)
				last = byteval;
			}
			else
			{
				// Second byte in a double-byte sequence.
				value = (last - 0xA1) * 0x5E;
				last = 0;
				if (byteval >= 0xA1 && byteval <= 0xFE)
				{
					value += (byteval - 0xA1);
				}
				else
				{
					// Invalid second byte.
					last = 0;
					length++;
					continue;
				}

				value *= 2;
				value = ((int) (table0208 [value]))
					| (((int) (table0208 [value + 1])) << 8);
				if (value == 0)
					value = ((int) (table0212 [value]))
						| (((int) (table0212 [value + 1])) << 8);
				if (value != 0)
					length++;
				else
					length++;
			}
		}

		// seems like .NET 2.0 adds \u30FB for insufficient
		// byte seuqence (for Japanese \u30FB makes sense).
		if (refresh && last != 0)
			length++;
		else
			last_count = last;

		// Return the final length to the caller.
		return length;
	}

	public override int GetChars (byte[] bytes, int byteIndex,
						 int byteCount, char[] chars,
						 int charIndex)
	{
		return GetChars (bytes, byteIndex, byteCount, chars, charIndex, false);
	}

	public override
	int GetChars (byte[] bytes, int byteIndex,
						 int byteCount, char[] chars,
						 int charIndex, bool refresh)
	{
		CheckRange (bytes, byteIndex, byteCount, chars, charIndex);

		// Decode the bytes in the buffer.
		int posn = charIndex;
		int charLength = chars.Length;
		int byteval, value;
		int last = last_bytes;
		byte[] table0208 = JISConvert.Convert.jisx0208ToUnicode;
		byte[] table0212 = JISConvert.Convert.jisx0212ToUnicode;

		while (byteCount > 0) {
			byteval = bytes [byteIndex++];
			--byteCount;
			if (last == 0) {
				if (byteval == 0x8F) {
					// SS3 (One-time triple-byte sequence) should follow.
					last = byteval;
				} else if (byteval <= 0x7F) {
					// Ordinary ASCII/Latin1/Control character.
					if (posn >= charLength)
						throw Insufficient ();
					chars [posn++] = (char) byteval;
				} else if (byteval == 0x8E) {
					// SS2 (One-time double-byte sequence) should follow.
					last = byteval;
				} else if (byteval >= 0xA1 && byteval <= 0xFE) {
					// First byte in a double-byte sequence.
					last = byteval;
				} else {
					// Invalid first byte.
					if (posn >= charLength)
						throw Insufficient ();
					chars [posn++] = '\u30FB';
				}
			}
			else if (last == 0x8E) {
				// SS2 (One-time double-byte sequence)
				if (byteval >= 0xA1 && byteval <= 0xDF) {
					value = ((byteval - 0x40) |
						(last + 0x71) << 8);
					if (posn >= charLength)
						throw Insufficient ();
					chars [posn++] = (char) value;
				} else {
					// Invalid second byte.
					if (posn >= charLength)
						throw Insufficient ();
					chars [posn++] = '\u30FB';
				}
				last =0;
			}
			else if (last == 0x8F) {
				// SS3: 3-byte character
				// FIXME: not supported (I don't think iso-2022-jp has)
				last = byteval;
			}
			else
			{
				// Second byte in a double-byte sequence.
				value = (last - 0xA1) * 0x5E;
				last = 0;
				if (byteval >= 0xA1 && byteval <= 0xFE)
				{
					value += (byteval - 0xA1);
				}
				else
				{
					// Invalid second byte.
					last = 0;
					if (posn >= charLength)
						throw Insufficient ();
					chars [posn++] = '\u30FB';
					continue;
				}

				value *= 2;
				value = ((int) (table0208 [value]))
					| (((int) (table0208 [value + 1])) << 8);
				if (value == 0)
					value = ((int) (table0212 [value]))
						| (((int) (table0212 [value + 1])) << 8);
				if (posn >= charLength)
					throw Insufficient ();
				if (value != 0)
					chars [posn++] = (char)value;
				else
					chars [posn++] = '\u30FB';
			}
		}

		if (refresh && last != 0) {
			// seems like .NET 2.0 adds \u30FB for insufficient
			// byte seuqence (for Japanese \u30FB makes sense).
			if (posn >= charLength)
				throw Insufficient ();
			chars [posn++] = '\u30FB';
		}
		else
			last_bytes = last;

		// Return the final length to the caller.
		return posn - charIndex;
	}

	Exception Insufficient ()
	{
		throw new ArgumentException
			(Strings.GetString
				("Arg_InsufficientSpace"), "chars");
	}
}; // class CP51932Decoder

[Serializable]
public class ENCeuc_jp : CP51932
{
	public ENCeuc_jp () : base() {}

}; // class ENCeucjp

}; // namespace I18N.CJK
