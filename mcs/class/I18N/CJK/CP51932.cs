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

	Well, there looks no jis.table source. Thus, it seems like it is 
	generated from text files from Unicode Home Page such like
	ftp://ftp.unicode.org/Public/MAPPINGS/OBSOLETE/EASTASIA/JIS/JIS0208.TXT
	However, it is non-normative and in Japan it is contains many problem.

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

public class CP51932 : Encoding
{
	// Magic number used by Windows for the EUC-JP code page.
	private const int EUC_JP_CODE_PAGE = 51932;

	// Internal state.
	private JISConvert convert;

	// Conversion cache (note that encoding is not thread safe)
	int lastByte;

	// Constructor.
	public CP51932 () : base (EUC_JP_CODE_PAGE)
	{
		// Load the JIS conversion tables.
		convert = JISConvert.Convert;
	}

	// Get the number of bytes needed to encode a character buffer.
	public override int GetByteCount (char [] chars, int index, int count)
	{
		// Validate the parameters.
		if (chars == null)
			throw new ArgumentNullException("chars");

		if (index < 0 || index > chars.Length)
			throw new ArgumentOutOfRangeException
				("index", Strings.GetString ("ArgRange_Array"));

		if (count < 0 || count > (chars.Length - index))
			throw new ArgumentOutOfRangeException
				("count", Strings.GetString ("ArgRange_Array"));

		// Determine the length of the final output.
		int length = 0;
		int ch, value;
		byte [] cjkToJis = convert.cjkToJis;
		byte [] extraToJis = convert.extraToJis;

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
				{
					++length;
				}
			} else if(ch >= 0xFF01 && ch <= 0xFFEF) {
				// This range contains extra characters,
				// including half-width katakana.
				value = (ch - 0xFF01) * 2;
				value = ((int)(extraToJis[value])) |
						(((int)(extraToJis[value + 1])) << 8);
			}
		}

		// Return the length to the caller.
		return length;
	}

	// Get the bytes that result from encoding a character buffer.
	public override int GetBytes (char[] chars, int charIndex, int charCount,
						 byte[] bytes, int byteIndex)
	{
		// Validate the parameters.
		if(chars == null)
		{
			throw new ArgumentNullException("chars");
		}
		if(bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if(charIndex < 0 || charIndex > chars.Length)
		{
			throw new ArgumentOutOfRangeException
				("charIndex", Strings.GetString("ArgRange_Array"));
		}
		if(charCount < 0 || charCount > (chars.Length - charIndex))
		{
			throw new ArgumentOutOfRangeException
				("charCount", Strings.GetString("ArgRange_Array"));
		}
		if(byteIndex < 0 || byteIndex > bytes.Length)
		{
			throw new ArgumentOutOfRangeException
				("byteIndex", Strings.GetString("ArgRange_Array"));
		}

		// Convert the characters into their byte form.
		int posn = byteIndex;
		int byteLength = bytes.Length;
		int ch, value;

		byte[] cjkToJis = convert.cjkToJis;
		byte[] greekToJis = convert.greekToJis;
		byte[] extraToJis = convert.extraToJis;

		while (charCount > 0) {
			ch = chars [charIndex++];
			--charCount;
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
			} else if (ch >= 0xFF01 && ch <= 0xFFEF) {
				// This range contains extra characters,
				// including half-width katakana.
				value = (ch - 0xFF01) * 2;
				value = ((int) (extraToJis [value])) |
						(((int) (extraToJis [value + 1])) << 8);
			} else {
				// Invalid character.
				value = 0;
			}

			if (value == 0) {
				bytes [posn++] = (byte) '?';
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
				// FIXME: JIS X 0212 support is not implemented.
				bytes[posn++] = (byte)'?';
				bytes[posn++] = (byte)'?';
			}
		}

		// Return the final length to the caller.
		return posn - byteIndex;
	}

	// Get the number of characters needed to decode a byte buffer.
	// TODO: check
	public override int GetCharCount (byte [] bytes, int index, int count)
	{
		// Validate the parameters.
		if (bytes == null)
			throw new ArgumentNullException ("bytes");

		if (index < 0 || index > bytes.Length)
			throw new ArgumentOutOfRangeException
				("index", Strings.GetString("ArgRange_Array"));

		if (count < 0 || count > (bytes.Length - index))
			throw new ArgumentOutOfRangeException
				("count", Strings.GetString("ArgRange_Array"));

		// Determine the total length of the converted string.
		int length = 0;
		int byteval;
		while (count > 0) {
			byteval = bytes [index++];
			--count;
			++length;

			if (byteval < 0x80) {
				// Ordinary ASCII/Latin1 character, or the
				// single-byte Yen or overline signs.
				continue;
			}
			else if (byteval == 0xFF) {
				if (count >= 2) {
					count -= 2;
					++length;
				} else {
					count--;
					++length; // "??" for invalid 3-byte character
				}
				continue;
			}
			if(count == 0) {
				// Missing second byte.
				continue;
			}
			++index;
			--count;
		}

		// Return the total length.
		return length;
	}

	public override int GetChars (byte[] bytes, int byteIndex,
						 int byteCount, char[] chars,
						 int charIndex)
	{
		// Validate the parameters.
		if(bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if(chars == null)
		{
			throw new ArgumentNullException("chars");
		}
		if(byteIndex < 0 || byteIndex > bytes.Length)
		{
			throw new ArgumentOutOfRangeException
				("byteIndex", Strings.GetString("ArgRange_Array"));
		}
		if(byteCount < 0 || byteCount > (bytes.Length - byteIndex))
		{
			throw new ArgumentOutOfRangeException
				("byteCount", Strings.GetString("ArgRange_Array"));
		}
		if(charIndex < 0 || charIndex > chars.Length)
		{
			throw new ArgumentOutOfRangeException
				("charIndex", Strings.GetString("ArgRange_Array"));
		}

		// Decode the bytes in the buffer.
		int posn = charIndex;
		int charLength = chars.Length;
		int byteval, value;
		int last = lastByte;
		byte[] table0208 = convert.jisx0208ToUnicode;
		byte[] table0212 = convert.jisx0212ToUnicode;

		while (byteCount > 0) {
			byteval = bytes [byteIndex++];
			--byteCount;
			if (last == 0) {
				if (posn >= charLength)
					throw new ArgumentException
						(Strings.GetString
							("Arg_InsufficientSpace"), "chars");

				if (byteval == 0x8F) {
					if (byteval != 0) {
						// Invalid second byte of a 3-byte character
						// FIXME: What should we do?
						last = 0;
					}
					// First byte in a triple-byte sequence
					else
						last = byteval;
				} else if (byteval <= 0x7F) {
					// Ordinary ASCII/Latin1/Control character.
					chars [posn++] = (char) byteval;
				} else if (byteval >= 0xA1 && byteval <= 0xFE) {
					// First byte in a double-byte sequence.
					last = byteval;
				} else {
					// Invalid first byte.
					chars [posn++] = '?';
				}
			}
			else if (last == 0x8F) {
				// 3-byte character
				// FIXME: currently not supported yet
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
					lastByte = 0;
					chars [posn++] = '?';
					continue;
				}

				value *= 2;
				value = ((int) (table0208 [value]))
					| (((int) (table0208 [value + 1])) << 8);
				if (value == 0)
					value = ((int) (table0212 [value]))
						| (((int) (table0212 [value + 1])) << 8);
				if (value != 0)
					chars [posn++] = (char)value;
				else
					chars [posn++] = '?';
			}
		}
		lastByte = last;

		// Return the final length to the caller.
		return posn - charIndex;
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

/* Use default implementation
	public override Decoder GetDecoder()
	{
		return new CP51932Decoder(convert);
	}
*/

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

	// Get the Windows code page represented by this object.
	public override int WindowsCodePage {
		get { return EUC_JP_CODE_PAGE; }
	}

#endif // !ECMA_COMPAT
}; // class CP51932

public class ENCeuc_jp : CP51932
{
	public ENCeuc_jp () : base() {}

}; // class ENCeucjp

}; // namespace I18N.CJK
