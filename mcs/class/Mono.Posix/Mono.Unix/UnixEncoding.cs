/*
 * Mono.Unix/UnixEncoding.cs
 *
 * Authors:
 *   Jonathan Pryor (jonpryor@vt.edu)
 *
 * Copyright (c) 2001, 2002  Southern Storm Software, Pty Ltd
 * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
 * Copyright (C) 2005 Jonathan Pryor
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

namespace Mono.Unix
{

using System;
using System.Text;

[Serializable]
public class UnixEncoding : Encoding
{
	public static readonly Encoding Instance = new UnixEncoding ();

	public static readonly char EscapeByte = '\u0000';

	// Constructors.
	public UnixEncoding ()
	{
	}

	// Internal version of "GetByteCount" which can handle a rolling
	// state between multiple calls to this method.
	private static int InternalGetByteCount (char[] chars, int index, int count, uint leftOver, bool flush)
	{
		// Validate the parameters.
		if (chars == null) {
			throw new ArgumentNullException ("chars");
		}
		if (index < 0 || index > chars.Length) {
			throw new ArgumentOutOfRangeException ("index", _("ArgRange_Array"));
		}
		if (count < 0 || count > (chars.Length - index)) {
			throw new ArgumentOutOfRangeException ("count", _("ArgRange_Array"));
		}

		// Determine the lengths of all characters.
		char ch;
		int length = 0;
		uint pair = leftOver;
		while (count > 0) {
			ch = chars[index];
			if (pair == 0) {
				if (ch == EscapeByte && count > 1) {
					++length;
					++index;
					--count;
				} else if (ch < '\u0080') {
					++length;
				} else if (ch < '\u0800') {
					length += 2;
				} else if (ch >= '\uD800' && ch <= '\uDBFF') {
					// This is the start of a surrogate pair.
					pair = (uint)ch;
				} else {
					length += 3;
				}
			} else if (ch >= '\uDC00' && ch <= '\uDFFF') {
				// We have a surrogate pair.
				length += 4;
				pair = 0;
			} else {
				// We have a surrogate start followed by a
				// regular character.  Technically, this is
				// invalid, but we have to do something.
				// We write out the surrogate start and then
				// re-visit the current character again.
				length += 3;
				pair = 0;
				continue;
			}
			++index;
			--count;
		}
		if (flush && pair != 0) {
			// Flush the left-over surrogate pair start.
			length += 3;
		}

		// Return the final length to the caller.
		return length;
	}

	// Get the number of bytes needed to encode a character buffer.
	public override int GetByteCount (char[] chars, int index, int count)
	{
		return InternalGetByteCount (chars, index, count, 0, true);
	}

	// Convenience wrappers for "GetByteCount".
	public override int GetByteCount (String s)
	{
		// Validate the parameters.
		if (s == null) {
			throw new ArgumentNullException ("s");
		}

		// Determine the lengths of all characters.
		char ch;
		int index = 0;
		int count = s.Length;
		int length = 0;
		uint pair;
		while (count > 0) {
			ch = s[index++];
			if (ch == EscapeByte && count > 1) {
				++length;
				++index;
				--count;
			} else if (ch < '\u0080') {
				++length;
			} else if (ch < '\u0800') {
				length += 2;
			} else if (ch >= '\uD800' && ch <= '\uDBFF' && count > 1) {
				// This may be the start of a surrogate pair.
				pair = (uint)(s[index]);
				if (pair >= (uint)0xDC00 && pair <= (uint)0xDFFF) {
					length += 4;
					++index;
					--count;
				} else {
					length += 3;
				}
			} else {
				length += 3;
			}
			--count;
		}

		// Return the final length to the caller.
		return length;
	}

	// Internal version of "GetBytes" which can handle a rolling
	// state between multiple calls to this method.
	private static int InternalGetBytes (char[] chars, int charIndex,
					     int charCount, byte[] bytes,
					     int byteIndex, ref uint leftOver,
					     bool flush)
	{
		// Validate the parameters.
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

		// Convert the characters into bytes.
		char ch;
		int length = bytes.Length;
		uint pair;
		uint left = leftOver;
		int posn = byteIndex;
		while (charCount > 0) {
			// Fetch the next UTF-16 character pair value.
			ch = chars[charIndex++];
			--charCount;
			if (left == 0) {
				if (ch >= '\uD800' && ch <= '\uDBFF') {
					// This is the start of a surrogate pair.
					left = (uint)ch;
					continue;
				} else if (ch == EscapeByte) {
					if (posn >= length) {
						throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
					}
					if (--charCount >= 0) {
						bytes[posn++] = (byte) chars [charIndex++];
					}
					continue;
				} else {
					// This is a regular character.
					pair = (uint)ch;
				}
			} else if (ch >= '\uDC00' && ch <= '\uDFFF') {
				// We have a surrogate pair.
				pair = ((left - (uint)0xD800) << 10) +
					   (((uint)ch) - (uint)0xDC00) +
					   (uint)0x10000;
				left = 0;
			} else {
				// We have a surrogate start followed by a
				// regular character.  Technically, this is
				// invalid, but we have to do something.
				// We write out the surrogate start and then
				// re-visit the current character again.
				pair = (uint)left;
				left = 0;
				--charIndex;
				++charCount;
			}

			// Encode the character pair value.
			if (pair < (uint)0x0080) {
				if (posn >= length) {
					throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
				}
				bytes[posn++] = (byte)pair;
			} else if (pair < (uint)0x0800) {
				if ((posn + 2) > length) {
					throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
				}
				bytes[posn++] = (byte)(0xC0 | (pair >> 6));
				bytes[posn++] = (byte)(0x80 | (pair & 0x3F));
			} else if (pair < (uint)0x10000) {
				if ((posn + 3) > length) {
					throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
				}
				bytes[posn++] = (byte)(0xE0 | (pair >> 12));
				bytes[posn++] = (byte)(0x80 | ((pair >> 6) & 0x3F));
				bytes[posn++] = (byte)(0x80 | (pair & 0x3F));
			} else {
				if ((posn + 4) > length) {
					throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
				}
				bytes[posn++] = (byte)(0xF0 | (pair >> 18));
				bytes[posn++] = (byte)(0x80 | ((pair >> 12) & 0x3F));
				bytes[posn++] = (byte)(0x80 | ((pair >> 6) & 0x3F));
				bytes[posn++] = (byte)(0x80 | (pair & 0x3F));
			}
		}
		if (flush && left != 0) {
			// Flush the left-over surrogate pair start.
			if ((posn + 3) > length) {
				throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
			}
			bytes[posn++] = (byte)(0xE0 | (left >> 12));
			bytes[posn++] = (byte)(0x80 | ((left >> 6) & 0x3F));
			bytes[posn++] = (byte)(0x80 | (left & 0x3F));
			left = 0;
		}
		leftOver = left;

		// Return the final count to the caller.
		return posn - byteIndex;
	}

	// Get the bytes that result from encoding a character buffer.
	public override int GetBytes (char[] chars, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
	{
		uint leftOver = 0;
		return InternalGetBytes (chars, charIndex, charCount, bytes, byteIndex, ref leftOver, true);
	}

	// Convenience wrappers for "GetBytes".
	public override int GetBytes (String s, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
	{
		// Validate the parameters.
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

		// Convert the characters into bytes.
		char ch;
		int length = bytes.Length;
		uint pair;
		int posn = byteIndex;
		while (charCount > 0) {
			// Fetch the next UTF-16 character pair value.
			ch = s[charIndex++];
			if (ch >= '\uD800' && ch <= '\uDBFF' && charCount > 1) {
				// This may be the start of a surrogate pair.
				pair = (uint)(s[charIndex]);
				if (pair >= (uint)0xDC00 && pair <= (uint)0xDFFF) {
					pair = (pair - (uint)0xDC00) +
						   ((((uint)ch) - (uint)0xD800) << 10) +
						   (uint)0x10000;
					++charIndex;
					--charCount;
				} else {
					pair = (uint)ch;
				}
			} else if (ch == EscapeByte && charCount > 1) {
				if (posn >= length) {
					throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
				}
				charCount -= 2;
				if (charCount >= 0) {
					bytes[posn++] = (byte) s [charIndex++];
				}
				continue;
			} else {
				pair = (uint)ch;
			}
			--charCount;

			// Encode the character pair value.
			if (pair < (uint)0x0080) {
				if (posn >= length) {
					throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
				}
				bytes[posn++] = (byte)pair;
			} else if (pair < (uint)0x0800) {
				if ((posn + 2) > length) {
					throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
				}
				bytes[posn++] = (byte)(0xC0 | (pair >> 6));
				bytes[posn++] = (byte)(0x80 | (pair & 0x3F));
			} else if (pair < (uint)0x10000) {
				if ((posn + 3) > length) {
					throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
				}
				bytes[posn++] = (byte)(0xE0 | (pair >> 12));
				bytes[posn++] = (byte)(0x80 | ((pair >> 6) & 0x3F));
				bytes[posn++] = (byte)(0x80 | (pair & 0x3F));
			} else {
				if ((posn + 4) > length) {
					throw new ArgumentException (_("Arg_InsufficientSpace"), "bytes");
				}
				bytes[posn++] = (byte)(0xF0 | (pair >> 18));
				bytes[posn++] = (byte)(0x80 | ((pair >> 12) & 0x3F));
				bytes[posn++] = (byte)(0x80 | ((pair >> 6) & 0x3F));
				bytes[posn++] = (byte)(0x80 | (pair & 0x3F));
			}
		}

		// Return the final count to the caller.
		return posn - byteIndex;
	}

	// Internal version of "GetCharCount" which can handle a rolling
	// state between multiple calls to this method.
	private static int InternalGetCharCount (byte[] bytes, int index, int count,
										   uint leftOverBits,
										   uint leftOverCount,
										   bool throwOnInvalid, bool flush)
	{
		// Validate the parameters.
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		if (index < 0 || index > bytes.Length) {
			throw new ArgumentOutOfRangeException ("index", _("ArgRange_Array"));
		}
		if (count < 0 || count > (bytes.Length - index)) {
			throw new ArgumentOutOfRangeException ("count", _("ArgRange_Array"));
		}

		// Determine the number of characters that we have.
		int next_raw = 0;
		uint ch;
		int length = 0;
		uint leftBits = leftOverBits;
		uint leftSoFar = (leftOverCount & (uint)0x0F);
		uint leftSize = ((leftOverCount >> 4) & (uint)0x0F);
		while (count > 0) {
			ch = (uint)(bytes[index++]);
			++next_raw;
			--count;
			if (leftSize == 0) {
				// Process a UTF-8 start character.
				if (ch < (uint)0x0080) {
					// Single-byte UTF-8 character.
					++length;
					next_raw = 0;
				} else if ((ch & (uint)0xE0) == (uint)0xC0) {
					// Double-byte UTF-8 character.
					leftBits = (ch & (uint)0x1F);
					leftSoFar = 1;
					leftSize = 2;
				} else if ((ch & (uint)0xF0) == (uint)0xE0) {
					// Three-byte UTF-8 character.
					leftBits = (ch & (uint)0x0F);
					leftSoFar = 1;
					leftSize = 3;
				} else if ((ch & (uint)0xF8) == (uint)0xF0) {
					// Four-byte UTF-8 character.
					leftBits = (ch & (uint)0x07);
					leftSoFar = 1;
					leftSize = 4;
				} else if ((ch & (uint)0xFC) == (uint)0xF8) {
					// Five-byte UTF-8 character.
					leftBits = (ch & (uint)0x03);
					leftSoFar = 1;
					leftSize = 5;
				} else if ((ch & (uint)0xFE) == (uint)0xFC) {
					// Six-byte UTF-8 character.
					leftBits = (ch & (uint)0x03);
					leftSoFar = 1;
					leftSize = 6;
				} else {
					// Invalid UTF-8 start character.
					if (throwOnInvalid) {
						// throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
					}
					length += next_raw*2;
					next_raw = 0;
				}
			} else {
				// Process an extra byte in a multi-byte sequence.
				if ((ch & (uint)0xC0) == (uint)0x80) {
					leftBits = ((leftBits << 6) | (ch & (uint)0x3F));
					if (++leftSoFar >= leftSize) {
						// We have a complete character now.
						if (leftBits < (uint)0x10000) {
							// is it an overlong ?
							bool overlong = false;
							switch (leftSize) {
							case 2:
								overlong = (leftBits <= 0x7F);
								break;
							case 3:
								overlong = (leftBits <= 0x07FF);
								break;
							case 4:
								overlong = (leftBits <= 0xFFFF);
								break;
							case 5:
								overlong = (leftBits <= 0x1FFFFF);
								break;
							case 6:
								overlong = (leftBits <= 0x03FFFFFF);
								break;
							}
							if (overlong) {
								// if (throwOnInvalid)
								// 	throw new ArgumentException (_("Overlong"), leftBits.ToString ());
								length += next_raw*2;
							}
							else
								++length;
						} else if (leftBits < (uint)0x110000) {
							length += 2;
						} else if (throwOnInvalid) {
							// ???
							// throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
							length += next_raw*2;
						}
						leftSize = 0;
						next_raw = 0;
					}
				} else {
					// Invalid UTF-8 sequence: clear and restart.
					if (throwOnInvalid) {
						// throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
					}
					// don't escape the current byte, process it normally
					if (ch < (uint)0x0080) {
						--index;
						++count;
						--next_raw;
					}
					length += next_raw*2;
					leftSize = 0;
					next_raw = 0;
				}
			}
		}
		if (flush && leftSize != 0 && throwOnInvalid) {
			// We had left-over bytes that didn't make up
			// a complete UTF-8 character sequence.
			// throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
			length += next_raw * 2;
		}

		// Return the final length to the caller.
		return length;
	}

	// Get the number of characters needed to decode a byte buffer.
	public override int GetCharCount (byte[] bytes, int index, int count)
	{
		return InternalGetCharCount (bytes, index, count, 0, 0, true, true);
	}

	// Get the characters that result from decoding a byte buffer.
	private static int InternalGetChars (byte[] bytes, int byteIndex,
									   int byteCount, char[] chars,
									   int charIndex, ref uint leftOverBits,
									   ref uint leftOverCount,
									   bool throwOnInvalid, bool flush)
	{
		// Validate the parameters.
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

		if (charIndex == chars.Length)
			return 0;

		// Convert the bytes into the output buffer.
		byte[] raw = new byte[6];
		int next_raw = 0;
		uint ch;
		int length = chars.Length;
		int posn = charIndex;
		uint leftBits = leftOverBits;
		uint leftSoFar = (leftOverCount & (uint)0x0F);
		uint leftSize = ((leftOverCount >> 4) & (uint)0x0F);
		while (byteCount > 0) {
			// Fetch the next character from the byte buffer.
			ch = (uint)(bytes[byteIndex++]);
			raw [next_raw++] = (byte) ch;
			--byteCount;
			if (leftSize == 0) {
				// Process a UTF-8 start character.
				if (ch < (uint)0x0080) {
					// Single-byte UTF-8 character.
					if (posn >= length) {
						throw new ArgumentException (_("Arg_InsufficientSpace"), "chars");
					}
					next_raw = 0;
					chars[posn++] = (char)ch;
				} else if ((ch & (uint)0xE0) == (uint)0xC0) {
					// Double-byte UTF-8 character.
					leftBits = (ch & (uint)0x1F);
					leftSoFar = 1;
					leftSize = 2;
				} else if ((ch & (uint)0xF0) == (uint)0xE0) {
					// Three-byte UTF-8 character.
					leftBits = (ch & (uint)0x0F);
					leftSoFar = 1;
					leftSize = 3;
				} else if ((ch & (uint)0xF8) == (uint)0xF0) {
					// Four-byte UTF-8 character.
					leftBits = (ch & (uint)0x07);
					leftSoFar = 1;
					leftSize = 4;
				} else if ((ch & (uint)0xFC) == (uint)0xF8) {
					// Five-byte UTF-8 character.
					leftBits = (ch & (uint)0x03);
					leftSoFar = 1;
					leftSize = 5;
				} else if ((ch & (uint)0xFE) == (uint)0xFC) {
					// Six-byte UTF-8 character.
					leftBits = (ch & (uint)0x03);
					leftSoFar = 1;
					leftSize = 6;
				} else {
					// Invalid UTF-8 start character.
					if (throwOnInvalid) {
						// throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
					}
					next_raw = 0;
					chars[posn++] = EscapeByte;
					chars[posn++] = (char) ch;
				}
			} else {
				// Process an extra byte in a multi-byte sequence.
				if ((ch & (uint)0xC0) == (uint)0x80) {
					leftBits = ((leftBits << 6) | (ch & (uint)0x3F));
					if (++leftSoFar >= leftSize) {
						// We have a complete character now.
						if (leftBits < (uint)0x10000) {
							// is it an overlong ?
							bool overlong = false;
							switch (leftSize) {
							case 2:
								overlong = (leftBits <= 0x7F);
								break;
							case 3:
								overlong = (leftBits <= 0x07FF);
								break;
							case 4:
								overlong = (leftBits <= 0xFFFF);
								break;
							case 5:
								overlong = (leftBits <= 0x1FFFFF);
								break;
							case 6:
								overlong = (leftBits <= 0x03FFFFFF);
								break;
							}
							if (overlong) {
								// if (throwOnInvalid)
								// 	throw new ArgumentException (_("Overlong"), leftBits.ToString ());
								CopyRaw (raw, ref next_raw, chars, ref posn, length);
							}
							else {
								if (posn >= length) {
									throw new ArgumentException
										(_("Arg_InsufficientSpace"), "chars");
								}
								chars[posn++] = (char)leftBits;
							}
						} else if (leftBits < (uint)0x110000) {
							if ((posn + 2) > length) {
								throw new ArgumentException
									(_("Arg_InsufficientSpace"), "chars");
							}
							leftBits -= (uint)0x10000;
							chars[posn++] = (char)((leftBits >> 10) +
												   (uint)0xD800);
							chars[posn++] =
								(char)((leftBits & (uint)0x3FF) + (uint)0xDC00);
						} else if (throwOnInvalid) {
							// ???
							// throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
							CopyRaw (raw, ref next_raw, chars, ref posn, length);
						}
						leftSize = 0;
						next_raw = 0;
					}
				} else {
					// Invalid UTF-8 sequence: clear and restart.
					if (throwOnInvalid) {
						// throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
					}
					// don't escape the current byte, process it normally
					if (ch < (uint)0x0080) {
						--byteIndex;
						++byteCount;
						--next_raw;
					}
					CopyRaw (raw, ref next_raw, chars, ref posn, length);
					leftSize = 0;
					next_raw = 0;
				}
			}
		}
		if (flush && leftSize != 0 && throwOnInvalid) {
			// We had left-over bytes that didn't make up
			// a complete UTF-8 character sequence.
			// throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
			CopyRaw (raw, ref next_raw, chars, ref posn, length);
		}
		leftOverBits = leftBits;
		leftOverCount = (leftSoFar | (leftSize << 4));

		// Return the final length to the caller.
		return posn - charIndex;
	}

	private static void CopyRaw (byte[] raw, ref int next_raw, char[] chars, ref int posn, int length)
	{
		if (posn+(next_raw*2) > length)
			throw new ArgumentException (_("Arg_InsufficientSpace"), "chars");

		for (int i = 0; i < next_raw; ++i) {
			chars[posn++] = EscapeByte;
			chars[posn++] = (char) raw [i];
		}

		next_raw = 0;
	}

	// Get the characters that result from decoding a byte buffer.
	public override int GetChars (byte[] bytes, int byteIndex, int byteCount,
								 char[] chars, int charIndex)
	{
		uint leftOverBits = 0;
		uint leftOverCount = 0;
		return InternalGetChars (bytes, byteIndex, byteCount, chars, 
				charIndex, ref leftOverBits, ref leftOverCount, true, true);
	}

	// Get the maximum number of bytes needed to encode a
	// specified number of characters.
	public override int GetMaxByteCount (int charCount)
	{
		if (charCount < 0) {
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_NonNegative"));
		}
		return charCount * 4;
	}

	// Get the maximum number of characters needed to decode a
	// specified number of bytes.
	public override int GetMaxCharCount (int byteCount)
	{
		if (byteCount < 0) {
			throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_NonNegative"));
		}
		return byteCount;
	}

	// Get a Unix-specific decoder that is attached to this instance.
	public override Decoder GetDecoder ()
	{
		return new UnixDecoder ();
	}

	// Get a Unix-specific encoder that is attached to this instance.
	public override Encoder GetEncoder ()
	{
		return new UnixEncoder ();
	}

	// Get the Unix preamble.
	public override byte[] GetPreamble ()
	{
		return new byte [0];
	}

	// Determine if this object is equal to another.
	public override bool Equals (Object value)
	{
		UnixEncoding enc = (value as UnixEncoding);
		if (enc != null) {
			return true;
		}
		else {
			return false;
		}
	}

	// Get the hash code for this object.
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
	
	public override byte [] GetBytes (String s)
	{
		if (s == null)
			throw new ArgumentNullException ("s");
		
		int length = GetByteCount (s);
		byte [] bytes = new byte [length];
		GetBytes (s, 0, s.Length, bytes, 0);
		return bytes;
	}

	// Unix decoder implementation.
	[Serializable]
	private class UnixDecoder : Decoder
	{
		private uint leftOverBits;
		private uint leftOverCount;

		// Constructor.
		public UnixDecoder ()
		{
			leftOverBits = 0;
			leftOverCount = 0;
		}

		// Override inherited methods.
		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			return InternalGetCharCount (bytes, index, count,
					leftOverBits, leftOverCount, true, false);
		}
		public override int GetChars (byte[] bytes, int byteIndex,
						 int byteCount, char[] chars, int charIndex)
		{
			return InternalGetChars (bytes, byteIndex, byteCount,
				chars, charIndex, ref leftOverBits, ref leftOverCount, true, false);
		}

	}

	// Unix encoder implementation.
	[Serializable]
	private class UnixEncoder : Encoder
	{
		private uint leftOver;

		// Constructor.
		public UnixEncoder ()
		{
			leftOver = 0;
		}

		// Override inherited methods.
		public override int GetByteCount (char[] chars, int index,
					 int count, bool flush)
		{
			return InternalGetByteCount (chars, index, count, leftOver, flush);
		}
		public override int GetBytes (char[] chars, int charIndex,
					 int charCount, byte[] bytes, int byteCount, bool flush)
		{
			int result;
			result = InternalGetBytes (chars, charIndex, charCount, bytes, byteCount, ref leftOver, flush);
			return result;
		}
	}

	private static string _ (string arg)
	{
		return arg;
	}
}
}

