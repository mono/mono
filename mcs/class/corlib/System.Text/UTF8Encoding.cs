/*
 * UTF8Encoding.cs - Implementation of the "System.Text.UTF8Encoding" class.
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
public class UTF8Encoding : Encoding
{
	// Magic number used by Windows for UTF-8.
	internal const int UTF8_CODE_PAGE = 65001;

	// Internal state.
	private bool emitIdentifier;
	private bool throwOnInvalid;

	// Constructors.
	public UTF8Encoding () : this (false, false) {}
	public UTF8Encoding (bool encoderShouldEmitUTF8Identifier)
			: this (encoderShouldEmitUTF8Identifier, false) {}
	public UTF8Encoding (bool encoderShouldEmitUTF8Identifier, bool throwOnInvalidBytes)
		: base (UTF8_CODE_PAGE)
	{
		emitIdentifier = encoderShouldEmitUTF8Identifier;
		throwOnInvalid = throwOnInvalidBytes;
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
				if (ch < '\u0080') {
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
			if (ch < '\u0080') {
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
			--charCount;
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
			} else {
				pair = (uint)ch;
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
		uint ch;
		int length = 0;
		uint leftBits = leftOverBits;
		uint leftSoFar = (leftOverCount & (uint)0x0F);
		uint leftSize = ((leftOverCount >> 4) & (uint)0x0F);
		while (count > 0) {
			ch = (uint)(bytes[index++]);
			--count;
			if (leftSize == 0) {
				// Process a UTF-8 start character.
				if (ch < (uint)0x0080) {
					// Single-byte UTF-8 character.
					++length;
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
				} else if ((ch & (uint)0xFC) == (uint)0xFC) {
					// Six-byte UTF-8 character.
					leftBits = (ch & (uint)0x03);
					leftSoFar = 1;
					leftSize = 6;
				} else {
					// Invalid UTF-8 start character.
					if (throwOnInvalid) {
						throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
					}
				}
			} else {
				// Process an extra byte in a multi-byte sequence.
				if ((ch & (uint)0xC0) == (uint)0x80) {
					leftBits = ((leftBits << 6) | (ch & (uint)0x3F));
					if (++leftSoFar >= leftSize) {
						// We have a complete character now.
						if (leftBits < (uint)0x10000) {
							if (leftBits != (uint)0xFEFF) {
								++length;
							}
						} else if (leftBits < (uint)0x110000) {
							length += 2;
						} else if (throwOnInvalid) {
							throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
						}
						leftSize = 0;
					}
				} else {
					// Invalid UTF-8 sequence: clear and restart.
					if (throwOnInvalid) {
						throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
					}
					leftSize = 0;
					--index;
					++count;
				}
			}
		}
		if (flush && leftSize != 0 && throwOnInvalid) {
			// We had left-over bytes that didn't make up
			// a complete UTF-8 character sequence.
			throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
		}

		// Return the final length to the caller.
		return length;
	}

	// Get the number of characters needed to decode a byte buffer.
	public override int GetCharCount (byte[] bytes, int index, int count)
	{
		return InternalGetCharCount (bytes, index, count, 0, 0, throwOnInvalid, true);
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

		// Convert the bytes into the output buffer.
		uint ch;
		int length = chars.Length;
		int posn = charIndex;
		uint leftBits = leftOverBits;
		uint leftSoFar = (leftOverCount & (uint)0x0F);
		uint leftSize = ((leftOverCount >> 4) & (uint)0x0F);
		while (byteCount > 0) {
			// Fetch the next character from the byte buffer.
			ch = (uint)(bytes[byteIndex++]);
			--byteCount;
			if (leftSize == 0) {
				// Process a UTF-8 start character.
				if (ch < (uint)0x0080) {
					// Single-byte UTF-8 character.
					if (posn >= length) {
						throw new ArgumentException (_("Arg_InsufficientSpace"), "chars");
					}
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
				} else if ((ch & (uint)0xFC) == (uint)0xFC) {
					// Six-byte UTF-8 character.
					leftBits = (ch & (uint)0x03);
					leftSoFar = 1;
					leftSize = 6;
				} else {
					// Invalid UTF-8 start character.
					if (throwOnInvalid) {
						throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
					}
				}
			} else {
				// Process an extra byte in a multi-byte sequence.
				if ((ch & (uint)0xC0) == (uint)0x80) {
					leftBits = ((leftBits << 6) | (ch & (uint)0x3F));
					if (++leftSoFar >= leftSize) {
						// We have a complete character now.
						if (leftBits < (uint)0x10000) {
							if (leftBits != (uint)0xFEFF) {
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
							throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
						}
						leftSize = 0;
					}
				} else {
					// Invalid UTF-8 sequence: clear and restart.
					if (throwOnInvalid) {
						throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
					}
					leftSize = 0;
					--byteIndex;
					++byteCount;
				}
			}
		}
		if (flush && leftSize != 0 && throwOnInvalid) {
			// We had left-over bytes that didn't make up
			// a complete UTF-8 character sequence.
			throw new ArgumentException (_("Arg_InvalidUTF8"), "bytes");
		}
		leftOverBits = leftBits;
		leftOverCount = (leftSoFar | (leftSize << 4));

		// Return the final length to the caller.
		return posn - charIndex;
	}

	// Get the characters that result from decoding a byte buffer.
	public override int GetChars (byte[] bytes, int byteIndex, int byteCount,
								 char[] chars, int charIndex)
	{
		uint leftOverBits = 0;
		uint leftOverCount = 0;
		return InternalGetChars (bytes, byteIndex, byteCount, chars, 
				charIndex, ref leftOverBits, ref leftOverCount, throwOnInvalid, true);
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

	// Get a UTF8-specific decoder that is attached to this instance.
	public override Decoder GetDecoder ()
	{
		return new UTF8Decoder (throwOnInvalid);
	}

	// Get a UTF8-specific encoder that is attached to this instance.
	public override Encoder GetEncoder ()
	{
		return new UTF8Encoder (emitIdentifier);
	}

	// Get the UTF8 preamble.
	public override byte[] GetPreamble ()
	{
		if (emitIdentifier) {
			byte[] pre = new byte [3];
			pre[0] = (byte)0xEF;
			pre[1] = (byte)0xBB;
			pre[2] = (byte)0xBF;
			return pre;
		} else {
			return new byte [0];
		}
	}

	// Determine if this object is equal to another.
	public override bool Equals (Object value)
	{
		UTF8Encoding enc = (value as UTF8Encoding);
		if (enc != null) {
			return (codePage == enc.codePage &&
					emitIdentifier == enc.emitIdentifier &&
					throwOnInvalid == enc.throwOnInvalid);
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
			return "utf-8";
		}
	}

	// Get the human-readable name for this encoding.
	public override String EncodingName
	{
		get {
			return "Unicode (UTF-8)";
		}
	}

	// Get the mail agent header name for this encoding.
	public override String HeaderName
	{
		get {
			return "utf-8";
		}
	}

	// Determine if this encoding can be displayed in a Web browser.
	public override bool IsBrowserDisplay
	{
		get {
			return true;
		}
	}

	// Determine if this encoding can be saved from a Web browser.
	public override bool IsBrowserSave
	{
		get {
			return true;
		}
	}

	// Determine if this encoding can be displayed in a mail/news agent.
	public override bool IsMailNewsDisplay
	{
		get {
			return true;
		}
	}

	// Determine if this encoding can be saved from a mail/news agent.
	public override bool IsMailNewsSave
	{
		get {
			return true;
		}
	}

	// Get the IANA-preferred Web name for this encoding.
	public override String WebName
	{
		get {
			return "utf-8";
		}
	}

	// Get the Windows code page represented by this object.
	public override int WindowsCodePage
	{
		get {
			return UnicodeEncoding.UNICODE_CODE_PAGE;
		}
	}

#endif // !ECMA_COMPAT

	// UTF-8 decoder implementation.
	[Serializable]
	private class UTF8Decoder : Decoder
	{
		private bool throwOnInvalid;
		private uint leftOverBits;
		private uint leftOverCount;

		// Constructor.
		public UTF8Decoder (bool throwOnInvalid)
		{
			this.throwOnInvalid = throwOnInvalid;
			leftOverBits = 0;
			leftOverCount = 0;
		}

		// Override inherited methods.
		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			return InternalGetCharCount (bytes, index, count,
					leftOverBits, leftOverCount, throwOnInvalid, false);
		}
		public override int GetChars (byte[] bytes, int byteIndex,
						 int byteCount, char[] chars, int charIndex)
		{
			return InternalGetChars (bytes, byteIndex, byteCount,
				chars, charIndex, ref leftOverBits, ref leftOverCount, throwOnInvalid, false);
		}

	} // class UTF8Decoder

	// UTF-8 encoder implementation.
	[Serializable]
	private class UTF8Encoder : Encoder
	{
		private bool emitIdentifier;
		private uint leftOver;

		// Constructor.
		public UTF8Encoder (bool emitIdentifier)
		{
			this.emitIdentifier = emitIdentifier;
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
			emitIdentifier = false;
			return result;
		}

	} // class UTF8Encoder

}; // class UTF8Encoding

}; // namespace System.Text
