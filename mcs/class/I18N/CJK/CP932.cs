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

namespace I18N.CJK
{

using System;
using System.Text;
using I18N.Common;

public unsafe class CP932 : Encoding
{
	// Magic number used by Windows for the Shift-JIS code page.
	private const int SHIFTJIS_CODE_PAGE = 932;

	// Internal state.
	private JISConvert convert;

	// Constructor.
	public CP932() : base(SHIFTJIS_CODE_PAGE)
			{
				// Load the JIS conversion tables.
				convert = JISConvert.Convert;
			}

	// Get the number of bytes needed to encode a character buffer.
	public override int GetByteCount(char[] chars, int index, int count)
			{
				// Validate the parameters.
				if(chars == null)
				{
					throw new ArgumentNullException("chars");
				}
				if(index < 0 || index > chars.Length)
				{
					throw new ArgumentOutOfRangeException
						("index", Strings.GetString("ArgRange_Array"));
				}
				if(count < 0 || count > (chars.Length - index))
				{
					throw new ArgumentOutOfRangeException
						("count", Strings.GetString("ArgRange_Array"));
				}

				// Determine the length of the final output.
				int length = 0;
				int ch, value;
#if __PNET__
				byte *cjkToJis = convert.cjkToJis;
				byte *extraToJis = convert.extraToJis;
#else
				byte[] cjkToJis = convert.cjkToJis;
				byte[] extraToJis = convert.extraToJis;
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
	public override int GetBytes(char[] chars, int charIndex, int charCount,
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
#if __PNET__
				byte *cjkToJis = convert.cjkToJis;
				byte *greekToJis = convert.greekToJis;
				byte *extraToJis = convert.extraToJis;
#else
				byte[] cjkToJis = convert.cjkToJis;
				byte[] greekToJis = convert.greekToJis;
				byte[] extraToJis = convert.extraToJis;
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
							// Invalid character.
							bytes[posn++] = (byte)'?';
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
					else if(ch >= 0xFF01 && ch <= 0xFFEF)
					{
						// This range contains extra characters,
						// including half-width katakana.
						value = (ch - 0xFF01) * 2;
						value = ((int)(extraToJis[value])) |
								(((int)(extraToJis[value + 1])) << 8);
					}
					else
					{
						// Invalid character.
						value = 0;
					}
					if(value == 0)
					{
						bytes[posn++] = (byte)'?';
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

	// Get the number of characters needed to decode a byte buffer.
	public override int GetCharCount(byte[] bytes, int index, int count)
			{
				// Validate the parameters.
				if(bytes == null)
				{
					throw new ArgumentNullException("bytes");
				}
				if(index < 0 || index > bytes.Length)
				{
					throw new ArgumentOutOfRangeException
						("index", Strings.GetString("ArgRange_Array"));
				}
				if(count < 0 || count > (bytes.Length - index))
				{
					throw new ArgumentOutOfRangeException
						("count", Strings.GetString("ArgRange_Array"));
				}

				// Determine the total length of the converted string.
				int length = 0;
				int byteval;
				while(count > 0)
				{
					byteval = bytes[index++];
					--count;
					++length;
					if(byteval < 0x80)
					{
						// Ordinary ASCII/Latin1 character, or the
						// single-byte Yen or overline signs.
						continue;
					}
					else if(byteval >= 0xA1 && byteval <= 0xDF)
					{
						// Half-width katakana.
						continue;
					}
					else if(byteval < 0x81 ||
					        (byteval > 0x9F && byteval < 0xE0) ||
							byteval > 0xEF)
					{
						// Invalid first byte.
						continue;
					}
					if(count == 0)
					{
						// Missing second byte.
						continue;
					}
					++index;
					--count;
				}

				// Return the total length.
				return length;
			}

	// Get the characters that result from decoding a byte buffer.
	public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
								 char[] chars, int charIndex)
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

				// Determine the total length of the converted string.
				int charLength = chars.Length;
				int posn = charIndex;
				int length = 0;
				int byteval, value;
#if __PNET__
				byte *table = convert.jisx0208ToUnicode;
#else
				byte[] table = convert.jisx0208ToUnicode;
#endif
				while(byteCount > 0)
				{
					byteval = bytes[byteIndex++];
					--byteCount;
					++length;
					if(posn >= charLength)
					{
						throw new ArgumentException
							(Strings.GetString("Arg_InsufficientSpace"),
							 "chars");
					}
					if(byteval == 0x5C)
					{
						// Yen sign.
						chars[posn++] = '\u00A5';
						continue;
					}
					else if(byteval == 0x7E)
					{
						// Overline symbol.
						chars[posn++] = '\u203E';
						continue;
					}
					else if(byteval < 0x80)
					{
						// Ordinary ASCII/Latin1 character.
						chars[posn++] = (char)byteval;
						continue;
					}
					else if(byteval >= 0xA1 && byteval <= 0xDF)
					{
						// Half-width katakana.
						chars[posn++] = (char)(byteval - 0xA1 + 0xFF61);
						continue;
					}
					else if(byteval >= 0x81 && byteval <= 0x9F)
					{
						value = (byteval - 0x81) * 0xBC;
					}
					else if(byteval >= 0xE0 && byteval <= 0xEF)
					{
						value = (byteval - 0xE0 + (0xA0 - 0x81)) * 0xBC;
					}
					else
					{
						// Invalid first byte.
						chars[posn++] = '?';
						continue;
					}
					if(byteCount == 0)
					{
						// Missing second byte.
						chars[posn++] = '?';
						continue;
					}
					byteval = bytes[byteIndex++];
					--byteCount;
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

				// Return the total length.
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
				return new CP932Decoder(convert);
			}

#if !ECMA_COMPAT

	// Get the mail body name for this encoding.
	public override String BodyName
			{
				get
				{
					return "iso-2022-jp";
				}
			}

	// Get the human-readable name for this encoding.
	public override String EncodingName
			{
				get
				{
					return "Japanese (Shift-JIS)";
				}
			}

	// Get the mail agent header name for this encoding.
	public override String HeaderName
			{
				get
				{
					return "iso-2022-jp";
				}
			}

	// Determine if this encoding can be displayed in a Web browser.
	public override bool IsBrowserDisplay
			{
				get
				{
					return true;
				}
			}

	// Determine if this encoding can be saved from a Web browser.
	public override bool IsBrowserSave
			{
				get
				{
					return true;
				}
			}

	// Determine if this encoding can be displayed in a mail/news agent.
	public override bool IsMailNewsDisplay
			{
				get
				{
					return true;
				}
			}

	// Determine if this encoding can be saved from a mail/news agent.
	public override bool IsMailNewsSave
			{
				get
				{
					return true;
				}
			}

	// Get the IANA-preferred Web name for this encoding.
	public override String WebName
			{
				get
				{
					return "shift_jis";
				}
			}

	// Get the Windows code page represented by this object.
	public override int WindowsCodePage
			{
				get
				{
					return SHIFTJIS_CODE_PAGE;
				}
			}

#endif // !ECMA_COMPAT

	// Decoder that handles a rolling Shift-JIS state.
	private sealed class CP932Decoder : Decoder
	{
		private JISConvert convert;
		private int lastByte;

		// Constructor.
		public CP932Decoder(JISConvert convert)
				{
					this.convert = convert;
					this.lastByte = 0;
				}

		// Override inherited methods.
		public override int GetCharCount(byte[] bytes, int index, int count)
				{
					// Validate the parameters.
					if(bytes == null)
					{
						throw new ArgumentNullException("bytes");
					}
					if(index < 0 || index > bytes.Length)
					{
						throw new ArgumentOutOfRangeException
							("index", Strings.GetString("ArgRange_Array"));
					}
					if(count < 0 || count > (bytes.Length - index))
					{
						throw new ArgumentOutOfRangeException
							("count", Strings.GetString("ArgRange_Array"));
					}

					// Determine the total length of the converted string.
					int length = 0;
					int byteval;
					int last = lastByte;
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
	
					// Return the total length.
					return length;
				}
		public override int GetChars(byte[] bytes, int byteIndex,
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
							else if(byteval == 0x5C)
							{
								// Yen sign.
								chars[posn++] ='\u00A5';
							}
							else if(byteval == 0x7E)
							{
								// Overline symbol.
								chars[posn++] ='\u203E';
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
					lastByte = last;

					// Return the final length to the caller.
					return posn - charIndex;
				}

	} // class CP932Decoder

}; // class CP932

public class ENCshift_jis : CP932
{
	public ENCshift_jis() : base() {}

}; // class ENCshift_jis

}; // namespace I18N.CJK
