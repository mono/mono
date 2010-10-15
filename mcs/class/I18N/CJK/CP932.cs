/*
 * CP932.cs - Japanese (Shift-JIS) code page.
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

//
// Copyright (C) 2005-2006 Novell, Inc.
//

namespace I18N.CJK
{

	using System;
	using System.Text;
	using I18N.Common;

	[Serializable]
	public unsafe class CP932 : MonoEncoding
	{
		// Magic number used by Windows for the Shift-JIS code page.
		private const int SHIFTJIS_CODE_PAGE = 932;

		// Constructor.
		public CP932() : base(SHIFTJIS_CODE_PAGE)
		{
		}

		// Get the number of bytes needed to encode a character buffer.
		public unsafe override int GetByteCountImpl (char* chars, int count)
		{
			int index = 0;

			// Determine the length of the final output.
			int length = 0;
			int ch, value;
#if __PNET__
			byte *cjkToJis = JISConvert.Convert.cjkToJis;
			byte *extraToJis = JISConvert.Convert.extraToJis;
#else
			byte[] cjkToJis = JISConvert.Convert.cjkToJis;
			byte[] extraToJis = JISConvert.Convert.extraToJis;
#endif
			while(count > 0)
			{
				ch = chars[index++];
				--count;
				++length;
				if(ch < 0x0080)
				{
					// Character maps to itself.
					continue;
				}
				else if(ch < 0x0100)
				{
					// Check for special Latin 1 characters that
					// can be mapped to double-byte code points.
					if(ch == 0x00A2 || ch == 0x00A3 || ch == 0x00A7 ||
					   ch == 0x00A8 || ch == 0x00AC || ch == 0x00B0 ||
					   ch == 0x00B1 || ch == 0x00B4 || ch == 0x00B6 ||
					   ch == 0x00D7 || ch == 0x00F7)
					{
						++length;
					}
				}
				else if(ch >= 0x0391 && ch <= 0x0451)
				{
					// Greek subset characters.
					++length;
				}
				else if(ch >= 0x2010 && ch <= 0x9FA5)
				{
					// This range contains the bulk of the CJK set.
					value = (ch - 0x2010) * 2;
					value = ((int)(cjkToJis[value])) |
							(((int)(cjkToJis[value + 1])) << 8);
					if(value >= 0x0100)
					{
						++length;
					}
				}
				else if(ch >= 0xE000 && ch <= 0xE757)
					// PrivateUse
					++length;
				else if(ch >= 0xFF01 && ch <= 0xFFEF)
				{
					// This range contains extra characters,
					// including half-width katakana.
					value = (ch - 0xFF01) * 2;
					value = ((int)(extraToJis[value])) |
							(((int)(extraToJis[value + 1])) << 8);
					if(value >= 0x0100)
					{
						++length;
					}
				}
			}

			// Return the length to the caller.
			return length;
		}

		// Get the bytes that result from encoding a character buffer.
		public unsafe override int GetBytesImpl (
			char* chars, int charCount, byte* bytes, int byteCount)
		{
			int charIndex = 0;
			int byteIndex = 0;
#if NET_2_0
			EncoderFallbackBuffer buffer = null;
#endif

			// Convert the characters into their byte form.
			int posn = byteIndex;
			int byteLength = byteCount;
			int ch, value;
#if __PNET__
			byte *cjkToJis = JISConvert.Convert.cjkToJis;
			byte *greekToJis = JISConvert.Convert.greekToJis;
			byte *extraToJis = JISConvert.Convert.extraToJis;
#else
			byte[] cjkToJis = JISConvert.Convert.cjkToJis;
			byte[] greekToJis = JISConvert.Convert.greekToJis;
			byte[] extraToJis = JISConvert.Convert.extraToJis;
#endif
			while(charCount > 0)
			{
				ch = chars[charIndex++];
				--charCount;
				if(posn >= byteLength)
				{
					throw new ArgumentException
						(Strings.GetString("Arg_InsufficientSpace"),
						 "bytes");
				}
				if(ch < 0x0080)
				{
					// Character maps to itself.
					bytes[posn++] = (byte)ch;
					continue;
				}
				else if(ch < 0x0100)
				{
					// Check for special Latin 1 characters that
					// can be mapped to double-byte code points.
					if(ch == 0x00A2 || ch == 0x00A3 || ch == 0x00A7 ||
					   ch == 0x00A8 || ch == 0x00AC || ch == 0x00B0 ||
					   ch == 0x00B1 || ch == 0x00B4 || ch == 0x00B6 ||
					   ch == 0x00D7 || ch == 0x00F7)
					{
						if((posn + 1) >= byteLength)
						{
							throw new ArgumentException
								(Strings.GetString
									("Arg_InsufficientSpace"), "bytes");
						}
						switch(ch)
						{
						case 0x00A2:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0x91;
							break;

						case 0x00A3:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0x92;
							break;

						case 0x00A7:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0x98;
							break;

						case 0x00A8:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0x4E;
							break;

						case 0x00AC:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0xCA;
							break;

						case 0x00B0:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0x8B;
							break;

						case 0x00B1:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0x7D;
							break;

						case 0x00B4:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0x4C;
							break;

						case 0x00B6:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0xF7;
							break;

						case 0x00D7:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0x7E;
							break;

						case 0x00F7:
							bytes[posn++] = (byte)0x81;
							bytes[posn++] = (byte)0x80;
							break;
						}
					}
					else if(ch == 0x00A5)
					{
						// Yen sign.
						bytes[posn++] = (byte)0x5C;
					}
					else
					{
#if NET_2_0
						HandleFallback (ref buffer,
							chars, ref charIndex, ref charCount,
							bytes, ref posn, ref byteCount);
#else
						// Invalid character.
						bytes[posn++] = (byte)'?';
#endif
					}
					continue;
				}
				else if(ch >= 0x0391 && ch <= 0x0451)
				{
					// Greek subset characters.
					value = (ch - 0x0391) * 2;
					value = ((int)(greekToJis[value])) |
							(((int)(greekToJis[value + 1])) << 8);
				}
				else if(ch >= 0x2010 && ch <= 0x9FA5)
				{
					// This range contains the bulk of the CJK set.
					value = (ch - 0x2010) * 2;
					value = ((int)(cjkToJis[value])) |
							(((int)(cjkToJis[value + 1])) << 8);
				}
				else if(ch >= 0xE000 && ch <= 0xE757)
				{
					// PrivateUse
					int diff = ch - 0xE000;
					value = ((int) (diff / 0xBC) << 8)
						+ (diff % 0xBC)
						+ 0xF040;
					if (value % 0x100 >= 0x7F)
						value++;
				}
				else if(ch >= 0xFF01 && ch <= 0xFF60)
				{
					value = (ch - 0xFF01) * 2;
					value = ((int)(extraToJis[value])) |
							(((int)(extraToJis[value + 1])) << 8);
				}
				else if(ch >= 0xFF60 && ch <= 0xFFA0)
				{
					value = ch - 0xFF60 + 0xA0;
				}
				else
				{
					// Invalid character.
					value = 0;
				}
				if(value == 0)
				{
#if NET_2_0
					HandleFallback (ref buffer,
						chars, ref charIndex, ref charCount,
						bytes, ref posn, ref byteCount);
#else
					bytes[posn++] = (byte)'?';
#endif
				}
				else if(value < 0x0100)
				{
					bytes[posn++] = (byte)value;
				}
				else if((posn + 1) >= byteLength)
				{
					throw new ArgumentException
						(Strings.GetString("Arg_InsufficientSpace"),
						 "bytes");
				}
				else if(value < 0x8000)
				{
					// JIS X 0208 character.
					value -= 0x0100;
					ch = (value / 0xBC);
					value = (value % 0xBC) + 0x40;
					if(value >= 0x7F)
					{
						++value;
					}
					if(ch < (0x9F - 0x80))
					{
						bytes[posn++] = (byte)(ch + 0x81);
					}
					else
					{
						bytes[posn++] = (byte)(ch - (0x9F - 0x80) + 0xE0);
					}
					bytes[posn++] = (byte)value;
				}
				else if (value >= 0xF040 && value <= 0xF9FC)
				{
					// PrivateUse
					bytes[posn++] = (byte) (value / 0x100);
					bytes[posn++] = (byte) (value % 0x100);
				}
				else
				{
					// JIS X 0212 character, which Shift-JIS doesn't
					// support, but we've already allocated two slots.
					bytes[posn++] = (byte)'?';
					bytes[posn++] = (byte)'?';
				}
			}

			// Return the final length to the caller.
			return posn - byteIndex;
		}

		public override int GetCharCount (byte [] bytes, int index, int count)
		{
			return new CP932Decoder (JISConvert.Convert).GetCharCount (
				bytes, index, count, true);
		}

		public override int GetChars (
			byte [] bytes, int byteIndex, int byteCount,
			char [] chars, int charIndex)
		{
			return new CP932Decoder (JISConvert.Convert).GetChars (bytes,
				byteIndex, byteCount, chars, charIndex,
				true);
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
			return charCount * 2;
		}

		// Get the maximum number of characters needed to decode a
		// specified number of bytes.
		public override int GetMaxCharCount(int byteCount)
		{
			if(byteCount < 0)
			{
				throw new ArgumentOutOfRangeException
					("byteCount",
					 Strings.GetString("ArgRange_NonNegative"));
			}
			return byteCount;
		}

		// Get a decoder that handles a rolling Shift-JIS state.
		public override Decoder GetDecoder()
		{
			return new CP932Decoder(JISConvert.Convert);
		}

#if !ECMA_COMPAT

		// Get the mail body name for this encoding.
		public override String BodyName {
			get { return "iso-2022-jp"; }
		}

		// Get the human-readable name for this encoding.
		public override String EncodingName {
			get { return "Japanese (Shift-JIS)"; }
		}

		// Get the mail agent header name for this encoding.
		public override String HeaderName {
			get { return "iso-2022-jp"; }
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
			get { return "shift_jis"; }
		}

		// Get the Windows code page represented by this object.
		public override int WindowsCodePage {
			get { return SHIFTJIS_CODE_PAGE; }
		}

	}; // class CP932

#endif // !ECMA_COMPAT

	// Decoder that handles a rolling Shift-JIS state.
	sealed class CP932Decoder : DbcsEncoding.DbcsDecoder
	{
		private new JISConvert convert;
		private int last_byte_count;
		private int last_byte_chars;

		// Constructor.
		public CP932Decoder(JISConvert convert)
			: base (null)
		{
			this.convert = convert;
		}

		// Override inherited methods.

		public override int GetCharCount (
			byte [] bytes, int index, int count)
		{
			return GetCharCount (bytes, index, count, false);
		}

		public
#if NET_2_0
		override
#endif
		int GetCharCount (byte [] bytes, int index, int count, bool refresh)
		{
			CheckRange (bytes, index, count);

			// Determine the total length of the converted string.
			int length = 0;
			int byteval;
			int last = last_byte_count;
			while(count > 0)
			{
				byteval = bytes[index++];
				--count;
				if(last == 0)
				{
					if((byteval >= 0x81 && byteval <= 0x9F) ||
					   (byteval >= 0xE0 && byteval <= 0xEF))
					{
						// First byte in a double-byte sequence.
						last = byteval;
					}
					++length;
				}
				else
				{
					// Second byte in a double-byte sequence.
					last = 0;
				}
			}
			if (refresh) {
				if (last != 0)
					length++;
				last_byte_count = '\0';
			}
			else
				last_byte_count = last;

			// Return the total length.
			return length;
		}

		public override int GetChars (
			byte [] bytes, int byteIndex, int byteCount,
			char [] chars, int charIndex)
		{
			return GetChars (bytes, byteIndex, byteCount,
					 chars, charIndex, false);
		}

		public
#if NET_2_0
		override
#endif
		int GetChars (
			byte [] bytes, int byteIndex, int byteCount,
			char [] chars, int charIndex, bool refresh)
		{
			CheckRange (bytes, byteIndex, byteCount,
				chars, charIndex);

			// Decode the bytes in the buffer.
			int posn = charIndex;
			int charLength = chars.Length;
			int byteval, value;
			int last = last_byte_chars;
#if __PNET__
			byte *table = convert.jisx0208ToUnicode;
#else
			byte[] table = convert.jisx0208ToUnicode;
#endif
			while(byteCount > 0)
			{
				byteval = bytes[byteIndex++];
				--byteCount;
				if(last == 0)
				{
					if(posn >= charLength)
					{
						throw new ArgumentException
							(Strings.GetString
								("Arg_InsufficientSpace"), "chars");
					}
					if((byteval >= 0x81 && byteval <= 0x9F) ||
					   (byteval >= 0xE0 && byteval <= 0xEF))
					{
						// First byte in a double-byte sequence.
						last = byteval;
					}
					else if(byteval < 0x80)
					{
						// Ordinary ASCII/Latin1 character.
						chars[posn++] = (char)byteval;
					}
					else if(byteval >= 0xA1 && byteval <= 0xDF)
					{
						// Half-width katakana character.
						chars[posn++] = (char)(byteval - 0xA1 + 0xFF61);
					}
					else
					{
						// Invalid first byte.
						chars[posn++] = '?';
					}
				}
				else
				{
					// Second byte in a double-byte sequence.
					if(last >= 0x81 && last <= 0x9F)
					{
						value = (last - 0x81) * 0xBC;
					}
					else if (last >= 0xF0 && last <= 0xFC && byteval <= 0xFC)
					{
						// PrivateUse
						value = 0xE000 + (last - 0xF0) * 0xBC + byteval;
						if (byteval > 0x7F)
							value--;
					}
					else
					{
						value = (last - 0xE0 + (0xA0 - 0x81)) * 0xBC;
					}
					last = 0;
					if(byteval >= 0x40 && byteval <= 0x7E)
					{
						value += (byteval - 0x40);
					}
					else if(byteval >= 0x80 && byteval <= 0xFC)
					{
						value += (byteval - 0x80 + 0x3F);
					}
					else
					{
						// Invalid second byte.
						chars[posn++] = '?';
						continue;
					}
					value *= 2;
					value = ((int)(table[value])) |
							(((int)(table[value + 1])) << 8);
					if(value != 0)
					{
						chars[posn++] = (char)value;
					}
					else
					{
						chars[posn++] = '?';
					}
				}
			}
			if (refresh) {
				if (last != 0)
					chars[posn++] = '\u30FB';
				last_byte_chars = '\0';
			}
			else
				last_byte_chars = last;

			// Return the final length to the caller.
			return posn - charIndex;
		}

	} // class CP932Decoder

	[Serializable]
	public class ENCshift_jis : CP932
	{
		public ENCshift_jis() : base() {}

	}; // class ENCshift_jis

}; // namespace I18N.CJK
