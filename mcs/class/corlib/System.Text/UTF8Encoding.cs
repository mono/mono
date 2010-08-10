/*
 * UTF8Encoding.cs - Implementation of the "System.Text.UTF8Encoding" class.
 *
 * Copyright (c) 2001, 2002  Southern Storm Software, Pty Ltd
 * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
[MonoLimitation ("Serialization format not compatible with .NET")]
[ComVisible (true)]
public class UTF8Encoding : Encoding
{
	// Magic number used by Windows for UTF-8.
	internal const int UTF8_CODE_PAGE = 65001;

	// Internal state.
	private bool emitIdentifier;

	// Constructors.
	public UTF8Encoding () : this (false, false) {}
	public UTF8Encoding (bool encoderShouldEmitUTF8Identifier)
			: this (encoderShouldEmitUTF8Identifier, false) {}
	
	public UTF8Encoding (bool encoderShouldEmitUTF8Identifier, bool throwOnInvalidBytes)
		: base (UTF8_CODE_PAGE)
	{
		emitIdentifier = encoderShouldEmitUTF8Identifier;
		if (throwOnInvalidBytes)
			SetFallbackInternal (EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
		else
			SetFallbackInternal (EncoderFallback.StandardSafeFallback, DecoderFallback.StandardSafeFallback);

		web_name = body_name = header_name = "utf-8";
		encoding_name = "Unicode (UTF-8)";
		is_browser_save = true;
		is_browser_display = true;
		is_mail_news_display = true;
		is_mail_news_save = true;
		windows_code_page = UnicodeEncoding.UNICODE_CODE_PAGE;
	}

	#region GetByteCount()

	// Internal version of "GetByteCount" which can handle a rolling
	// state between multiple calls to this method.
	private static int InternalGetByteCount (char[] chars, int index, int count, EncoderFallback fallback, ref char leftOver, bool flush)
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

		if (index == chars.Length) {
			if (flush && leftOver != '\0') {
				// Flush the left-over surrogate pair start.
				leftOver = '\0';
				return 3;
			}
			return 0;
		}

		unsafe {
			fixed (char* cptr = chars) {
				return InternalGetByteCount (cptr + index, count, fallback, ref leftOver, flush);
			}
		}
	}

	private unsafe static int InternalGetByteCount (char* chars, int count, EncoderFallback fallback, ref char leftOver, bool flush)
	{
		int length = 0;
		char* end = chars + count;
		char* start = chars;
		EncoderFallbackBuffer buffer = null;
		while (chars < end) {
			if (leftOver == 0) {
				for (; chars < end; chars++) {
					if (*chars < '\x80') {
						++length;
					} else if (*chars < '\x800') {
						length += 2;
					} else if (*chars < '\uD800' || *chars > '\uDFFF') {
						length += 3;
					} else if (*chars <= '\uDBFF') {
						// This is a surrogate start char, exit the inner loop only
						// if we don't find the complete surrogate pair.
						if (chars + 1 < end && chars [1] >= '\uDC00' && chars [1] <= '\uDFFF') {
							length += 4;
							chars++;
							continue;
						}
						leftOver = *chars;
						chars++;
						break;
					} else {
						// We have a surrogate tail without 
						// leading surrogate. In NET_2_0 it
						// uses fallback. In NET_1_1 we output
						// wrong surrogate.
						char [] fallback_chars = GetFallbackChars (chars, start, fallback, ref buffer);
						fixed (char *fb_chars = fallback_chars) {
							char dummy = '\0';
							length += InternalGetByteCount (fb_chars, fallback_chars.Length, fallback, ref dummy, true);
						}

						leftOver = '\0';
					}
				}
			} else {
				if (*chars >= '\uDC00' && *chars <= '\uDFFF') {
					// We have a correct surrogate pair.
					length += 4;
					chars++;
				} else {
					// We have a surrogate start followed by a
					// regular character.  Technically, this is
					// invalid, but we have to do something.
					// We write out the surrogate start and then
					// re-visit the current character again.
					char [] fallback_chars = GetFallbackChars (chars, start, fallback, ref buffer);
					fixed (char *fb_chars = fallback_chars) {
						char dummy = '\0';
						length += InternalGetByteCount (fb_chars, fallback_chars.Length, fallback, ref dummy, true);
					}
				}
				leftOver = '\0';
			}
		}
		if (flush) {
			// Flush the left-over surrogate pair start.
			if (leftOver != '\0') {
				length += 3;
				leftOver = '\0';
			}
		}
		return length;
	}

	unsafe static char [] GetFallbackChars (char *chars, char *start, EncoderFallback fallback, ref EncoderFallbackBuffer buffer)
	{
		if (buffer == null)
			buffer = fallback.CreateFallbackBuffer ();

		buffer.Fallback (*chars, (int) (chars - start));

		char [] fallback_chars = new char [buffer.Remaining];
		for (int i = 0; i < fallback_chars.Length; i++)
			fallback_chars [i] = buffer.GetNextChar ();

		buffer.Reset ();

		return fallback_chars;
	}

	// Get the number of bytes needed to encode a character buffer.
	public override int GetByteCount (char[] chars, int index, int count)
	{
		char dummy = '\0';
		return InternalGetByteCount (chars, index, count, EncoderFallback, ref dummy, true);
	}


	[CLSCompliant (false)]
	[ComVisible (false)]
	public unsafe override int GetByteCount (char* chars, int count)
	{
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (count == 0)
			return 0;
		char dummy = '\0';
		return InternalGetByteCount (chars, count, EncoderFallback, ref dummy, true);
	}

	#endregion

	#region GetBytes()

	// Internal version of "GetBytes" which can handle a rolling
	// state between multiple calls to this method.
	private static int InternalGetBytes (char[] chars, int charIndex,
					     int charCount, byte[] bytes,
					     int byteIndex,
						 EncoderFallback fallback, ref EncoderFallbackBuffer buffer,
						 ref char leftOver, bool flush)
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

		if (charIndex == chars.Length) {
			if (flush && leftOver != '\0') {
				// FIXME: use EncoderFallback.
				//
				// By default it is empty, so I do nothing for now.
				leftOver = '\0';
			}
			return 0;
		}

		unsafe {
			fixed (char* cptr = chars) {
				if (bytes.Length == byteIndex)
					return InternalGetBytes (
						cptr + charIndex, charCount, 
						null, 0, fallback, ref buffer, ref leftOver, flush);
				fixed (byte *bptr = bytes) {
					return InternalGetBytes (
						cptr + charIndex, charCount,
						bptr + byteIndex, bytes.Length - byteIndex,
						fallback, ref buffer,
						ref leftOver, flush);
				}
			}
		}
	}

	private unsafe static int InternalGetBytes (char* chars, int count, byte* bytes, int bcount, EncoderFallback fallback, ref EncoderFallbackBuffer buffer, ref char leftOver, bool flush)
	{
		char* end = chars + count;
		char* start = chars;
		byte* start_bytes = bytes;
		byte* end_bytes = bytes + bcount;
		while (chars < end) {
			if (leftOver == 0) {
				for (; chars < end; chars++) {
					int ch = *chars;
					if (ch < '\x80') {
						if (bytes >= end_bytes)
							goto fail_no_space;
						*bytes++ = (byte)ch;
					} else if (ch < '\x800') {
						if (bytes + 1 >= end_bytes)
							goto fail_no_space;
						bytes [0] = (byte) (0xC0 | (ch >> 6));
						bytes [1] = (byte) (0x80 | (ch & 0x3F));
						bytes += 2;
					} else if (ch < '\uD800' || ch > '\uDFFF') {
						if (bytes + 2 >= end_bytes)
							goto fail_no_space;
						bytes [0] = (byte) (0xE0 | (ch >> 12));
						bytes [1] = (byte) (0x80 | ((ch >> 6) & 0x3F));
						bytes [2] = (byte) (0x80 | (ch & 0x3F));
						bytes += 3;
					} else if (ch <= '\uDBFF') {
						// This is a surrogate char, exit the inner loop.
						leftOver = *chars;
						chars++;
						break;
					} else {
						// We have a surrogate tail without 
						// leading surrogate. In NET_2_0 it
						// uses fallback. In NET_1_1 we output
						// wrong surrogate.
						char [] fallback_chars = GetFallbackChars (chars, start, fallback, ref buffer); 
						char dummy = '\0';
						if (bytes + InternalGetByteCount (fallback_chars, 0, fallback_chars.Length, fallback, ref dummy, true) > end_bytes)
							goto fail_no_space;
						fixed (char *fb_chars = fallback_chars) {
							bytes += InternalGetBytes (fb_chars, fallback_chars.Length, bytes, bcount - (int) (bytes - start_bytes), fallback, ref buffer, ref dummy, true);
						}

						leftOver = '\0';
					}
				}
			} else {
				if (*chars >= '\uDC00' && *chars <= '\uDFFF') {
					// We have a correct surrogate pair.
					int ch = 0x10000 + (int) *chars - 0xDC00 + (((int) leftOver - 0xD800) << 10);
					if (bytes + 3 >= end_bytes)
						goto fail_no_space;
					bytes [0] = (byte) (0xF0 | (ch >> 18));
					bytes [1] = (byte) (0x80 | ((ch >> 12) & 0x3F));
					bytes [2] = (byte) (0x80 | ((ch >> 6) & 0x3F));
					bytes [3] = (byte) (0x80 | (ch & 0x3F));
					bytes += 4;
					chars++;
				} else {
					// We have a surrogate start followed by a
					// regular character.  Technically, this is
					// invalid, but we have to do something.
					// We write out the surrogate start and then
					// re-visit the current character again.
					char [] fallback_chars = GetFallbackChars (chars, start, fallback, ref buffer); 
					char dummy = '\0';
					if (bytes + InternalGetByteCount (fallback_chars, 0, fallback_chars.Length, fallback, ref dummy, true) > end_bytes)
						goto fail_no_space;
					fixed (char *fb_chars = fallback_chars) {
						InternalGetBytes (fb_chars, fallback_chars.Length, bytes, bcount - (int) (bytes - start_bytes), fallback, ref buffer, ref dummy, true);
					}

					leftOver = '\0';
				}
				leftOver = '\0';
			}
		}
		if (flush) {
			// Flush the left-over surrogate pair start.
			if (leftOver != '\0') {
				int ch = leftOver;
				if (bytes + 2 < end_bytes) {
					bytes [0] = (byte) (0xE0 | (ch >> 12));
					bytes [1] = (byte) (0x80 | ((ch >> 6) & 0x3F));
					bytes [2] = (byte) (0x80 | (ch & 0x3F));
					bytes += 3;
				} else {
					goto fail_no_space;
				}
				leftOver = '\0';
			}
		}
		return (int)(bytes - (end_bytes - bcount));
fail_no_space:
		throw new ArgumentException ("Insufficient Space", "bytes");
	}

	// Get the bytes that result from encoding a character buffer.
	public override int GetBytes (char[] chars, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
	{
		char leftOver = '\0';
		EncoderFallbackBuffer buffer = null;
		return InternalGetBytes (chars, charIndex, charCount, bytes, byteIndex, EncoderFallback, ref buffer, ref leftOver, true);
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

		if (charIndex == s.Length)
			return 0;

		unsafe {
			fixed (char* cptr = s) {
				char dummy = '\0';
				EncoderFallbackBuffer buffer = null;
				if (bytes.Length == byteIndex)
					return InternalGetBytes (
						cptr + charIndex, charCount,
						null, 0, EncoderFallback, ref buffer, ref dummy, true);
				fixed (byte *bptr = bytes) {
					return InternalGetBytes (
						cptr + charIndex, charCount,
						bptr + byteIndex, bytes.Length - byteIndex,
						EncoderFallback, ref buffer,
						ref dummy, true);
				}
			}
		}
	}

	[CLSCompliant (false)]
	[ComVisible (false)]
	public unsafe override int GetBytes (char* chars, int charCount, byte* bytes, int byteCount)
	{
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (charCount < 0)
			throw new IndexOutOfRangeException ("charCount");
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (byteCount < 0)
			throw new IndexOutOfRangeException ("charCount");

		if (charCount == 0)
			return 0;

		char dummy = '\0';
		EncoderFallbackBuffer buffer = null;
		if (byteCount == 0)
			return InternalGetBytes (chars, charCount, null, 0, EncoderFallback, ref buffer, ref dummy, true);
		else
			return InternalGetBytes (chars, charCount, bytes, byteCount, EncoderFallback, ref buffer, ref dummy, true);
	}

	#endregion

	// Internal version of "GetCharCount" which can handle a rolling
	// state between multiple calls to this method.
	private unsafe static int InternalGetCharCount (
		byte[] bytes, int index, int count, uint leftOverBits,
		uint leftOverCount, object provider,
		ref DecoderFallbackBuffer fallbackBuffer, ref byte [] bufferArg, bool flush)
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

		if (count == 0)
			return 0;
		fixed (byte *bptr = bytes)
			return InternalGetCharCount (bptr + index, count,
				leftOverBits, leftOverCount, provider, ref fallbackBuffer, ref bufferArg, flush);
	}

	private unsafe static int InternalGetCharCount (
		byte* bytes, int count, uint leftOverBits,
		uint leftOverCount, object provider,
		ref DecoderFallbackBuffer fallbackBuffer, ref byte [] bufferArg, bool flush)
	{
		int index = 0;

		int length = 0;

		if (leftOverCount == 0) {
			int end = index + count;
			for (; index < end; index++, count--) {
				if (bytes [index] < 0x80)
					length++;
				else
					break;
			}
		}

		// Determine the number of characters that we have.
		uint ch;
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
				} else if ((ch & (uint)0xFE) == (uint)0xFC) {
					// Six-byte UTF-8 character.
					leftBits = (ch & (uint)0x03);
					leftSoFar = 1;
					leftSize = 6;
				} else {
					// Invalid UTF-8 start character.
					length += Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, index - 1, 1);
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
								length += Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, index - leftSoFar, leftSoFar);
							}
							else if ((leftBits & 0xF800) == 0xD800) {
								// UTF-8 doesn't use surrogate characters
								length += Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, index - leftSoFar, leftSoFar);
							}
							else
								++length;
						} else if (leftBits < (uint)0x110000) {
							length += 2;
						} else {
							length += Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, index - leftSoFar, leftSoFar);
						}
						leftSize = 0;
					}
				} else {
					// Invalid UTF-8 sequence: clear and restart.
					length += Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, index - leftSoFar, leftSoFar);
					leftSize = 0;
					--index;
					++count;
				}
			}
		}
		if (flush && leftSize != 0) {
			// We had left-over bytes that didn't make up
			// a complete UTF-8 character sequence.
			length += Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, index - leftSoFar, leftSoFar);
		}

		// Return the final length to the caller.
		return length;
	}

	// for GetCharCount()
	static unsafe int Fallback (object provider, ref DecoderFallbackBuffer buffer, ref byte [] bufferArg, byte* bytes, long index, uint size)
	{
		if (buffer == null) {
			DecoderFallback fb = provider as DecoderFallback;
			if (fb != null)
				buffer = fb.CreateFallbackBuffer ();
			else
				buffer = ((Decoder) provider).FallbackBuffer;
		}
		if (bufferArg == null)
			bufferArg = new byte [1];
		int ret = 0;
		for (int i = 0; i < size; i++) {
			bufferArg [0] = bytes [(int) index + i];
			buffer.Fallback (bufferArg, 0);
			ret += buffer.Remaining;
			buffer.Reset ();
		}
		return ret;
	}

	// for GetChars()
	static unsafe void Fallback (object provider, ref DecoderFallbackBuffer buffer, ref byte [] bufferArg, byte* bytes, long byteIndex, uint size,
		char* chars, ref int charIndex)
	{
		if (buffer == null) {
			DecoderFallback fb = provider as DecoderFallback;
			if (fb != null)
				buffer = fb.CreateFallbackBuffer ();
			else
				buffer = ((Decoder) provider).FallbackBuffer;
		}
		if (bufferArg == null)
			bufferArg = new byte [1];
		for (int i = 0; i < size; i++) {
			bufferArg [0] = bytes [byteIndex + i];
			buffer.Fallback (bufferArg, 0);
			while (buffer.Remaining > 0)
				chars [charIndex++] = buffer.GetNextChar ();
			buffer.Reset ();
		}
	}

	// Get the number of characters needed to decode a byte buffer.
	public override int GetCharCount (byte[] bytes, int index, int count)
	{
		DecoderFallbackBuffer buf = null;
		byte [] bufferArg = null;
		return InternalGetCharCount (bytes, index, count, 0, 0, DecoderFallback, ref buf, ref bufferArg, true);
	}

	[CLSCompliant (false)]
	[ComVisible (false)]
	public unsafe override int GetCharCount (byte* bytes, int count)
	{
		DecoderFallbackBuffer buf = null;
		byte [] bufferArg = null;
		return InternalGetCharCount (bytes, count, 0, 0, DecoderFallback, ref buf, ref bufferArg, true);
	}

	// Get the characters that result from decoding a byte buffer.
	private unsafe static int InternalGetChars (
		byte[] bytes, int byteIndex, int byteCount, char[] chars,
		int charIndex, ref uint leftOverBits, ref uint leftOverCount,
		object provider,
		ref DecoderFallbackBuffer fallbackBuffer, ref byte [] bufferArg, bool flush)
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

		fixed (char* cptr = chars) {
			if (byteCount == 0 || byteIndex == bytes.Length)
				return InternalGetChars (null, 0, cptr + charIndex, chars.Length - charIndex, ref leftOverBits, ref leftOverCount, provider, ref fallbackBuffer, ref bufferArg, flush);
			// otherwise...
			fixed (byte* bptr = bytes)
				return InternalGetChars (bptr + byteIndex, byteCount, cptr + charIndex, chars.Length - charIndex, ref leftOverBits, ref leftOverCount, provider, ref fallbackBuffer, ref bufferArg, flush);
		}
	}

	private unsafe static int InternalGetChars (
		byte* bytes, int byteCount, char* chars, int charCount,
		ref uint leftOverBits, ref uint leftOverCount,
		object provider,
		ref DecoderFallbackBuffer fallbackBuffer, ref byte [] bufferArg, bool flush)
	{
		int charIndex = 0, byteIndex = 0;
		int length = charCount;
		int posn = charIndex;

		if (leftOverCount == 0) {
			int end = byteIndex + byteCount;
			for (; byteIndex < end; posn++, byteIndex++, byteCount--) {
				if (bytes [byteIndex] < 0x80)
					chars [posn] = (char) bytes [byteIndex];
				else
					break;
			}
		}

		// Convert the bytes into the output buffer.
		uint ch;
		uint leftBits = leftOverBits;
		uint leftSoFar = (leftOverCount & (uint)0x0F);
		uint leftSize = ((leftOverCount >> 4) & (uint)0x0F);

		int byteEnd = byteIndex + byteCount;
		for(; byteIndex < byteEnd; byteIndex++) {
			// Fetch the next character from the byte buffer.
			ch = (uint)(bytes[byteIndex]);
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
				} else if ((ch & (uint)0xFE) == (uint)0xFC) {
					// Six-byte UTF-8 character.
					leftBits = (ch & (uint)0x03);
					leftSoFar = 1;
					leftSize = 6;
				} else {
					// Invalid UTF-8 start character.
					Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, byteIndex, 1, chars, ref posn);
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
								Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, byteIndex - leftSoFar, leftSoFar, chars, ref posn);
							}
							else if ((leftBits & 0xF800) == 0xD800) {
								// UTF-8 doesn't use surrogate characters
								Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, byteIndex - leftSoFar, leftSoFar, chars, ref posn);
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
						} else {
							Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, byteIndex - leftSoFar, leftSoFar, chars, ref posn);
						}
						leftSize = 0;
					}
				} else {
					// Invalid UTF-8 sequence: clear and restart.
					Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, byteIndex - leftSoFar, leftSoFar, chars, ref posn);
					leftSize = 0;
					--byteIndex;
				}
			}
		}
		if (flush && leftSize != 0) {
			// We had left-over bytes that didn't make up
			// a complete UTF-8 character sequence.
			Fallback (provider, ref fallbackBuffer, ref bufferArg, bytes, byteIndex - leftSoFar, leftSoFar, chars, ref posn);
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
		DecoderFallbackBuffer buf = null;
		byte [] bufferArg = null;
		return InternalGetChars (bytes, byteIndex, byteCount, chars, 
				charIndex, ref leftOverBits, ref leftOverCount, DecoderFallback, ref buf, ref bufferArg, true);
	}

	[CLSCompliant (false)]
	[ComVisible (false)]
	public unsafe override int GetChars (byte* bytes, int byteCount, char* chars, int charCount)
	{
		DecoderFallbackBuffer buf = null;
		byte [] bufferArg = null;
		uint leftOverBits = 0;
		uint leftOverCount = 0;
		return InternalGetChars (bytes, byteCount, chars, 
				charCount, ref leftOverBits, ref leftOverCount, DecoderFallback, ref buf, ref bufferArg, true);
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
		return new UTF8Decoder (DecoderFallback);
	}

	// Get a UTF8-specific encoder that is attached to this instance.
	public override Encoder GetEncoder ()
	{
		return new UTF8Encoder (EncoderFallback, emitIdentifier);
	}

	// Get the UTF8 preamble.
	public override byte[] GetPreamble ()
	{
		if (emitIdentifier)
			return new byte [] { 0xEF, 0xBB, 0xBF };

		return new byte [0];
	}

	// Determine if this object is equal to another.
	public override bool Equals (Object value)
	{
		UTF8Encoding enc = (value as UTF8Encoding);
		if (enc != null) {
			return (codePage == enc.codePage &&
				emitIdentifier == enc.emitIdentifier &&
				DecoderFallback.Equals (enc.DecoderFallback) &&
				EncoderFallback.Equals (enc.EncoderFallback));
		} else {
			return false;
		}
	}

	// Get the hash code for this object.
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}

	public override int GetByteCount (string chars)
	{
		// hmm, does this override make any sense?
		return base.GetByteCount (chars);
	}

	[ComVisible (false)]
	public override string GetString (byte [] bytes, int index, int count)
	{
		// hmm, does this override make any sense?
		return base.GetString (bytes, index, count);
	}

	// UTF-8 decoder implementation.
	[Serializable]
	private class UTF8Decoder : Decoder
	{
		private uint leftOverBits;
		private uint leftOverCount;

		// Constructor.
		public UTF8Decoder (DecoderFallback fallback)
		{
			Fallback = fallback;
			leftOverBits = 0;
			leftOverCount = 0;
		}

		// Override inherited methods.
		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			DecoderFallbackBuffer buf = null;
			byte [] bufferArg = null;
			return InternalGetCharCount (bytes, index, count,
				leftOverBits, leftOverCount, this, ref buf, ref bufferArg, false);
		}
		public override int GetChars (byte[] bytes, int byteIndex,
						 int byteCount, char[] chars, int charIndex)
		{
			DecoderFallbackBuffer buf = null;
			byte [] bufferArg = null;
			return InternalGetChars (bytes, byteIndex, byteCount,
				chars, charIndex, ref leftOverBits, ref leftOverCount, this, ref buf, ref bufferArg, false);
		}

	} // class UTF8Decoder

	// UTF-8 encoder implementation.
	[Serializable]
	private class UTF8Encoder : Encoder
	{
//		private bool emitIdentifier;
		private char leftOverForCount;
		private char leftOverForConv;

		// Constructor.
		public UTF8Encoder (EncoderFallback fallback, bool emitIdentifier)
		{
			Fallback = fallback;
//			this.emitIdentifier = emitIdentifier;
			leftOverForCount = '\0';
			leftOverForConv = '\0';
		}

		// Override inherited methods.
		public override int GetByteCount (char[] chars, int index,
					 int count, bool flush)
		{
			return InternalGetByteCount (chars, index, count, Fallback, ref leftOverForCount, flush);
		}
		public override int GetBytes (char[] chars, int charIndex,
					 int charCount, byte[] bytes, int byteIndex, bool flush)
		{
			int result;
			EncoderFallbackBuffer buffer = null;
			result = InternalGetBytes (chars, charIndex, charCount, bytes, byteIndex, Fallback, ref buffer, ref leftOverForConv, flush);
//			emitIdentifier = false;
			return result;
		}

		public unsafe override int GetByteCount (char* chars, int count, bool flush)
		{
			return InternalGetByteCount (chars, count, Fallback, ref leftOverForCount, flush);
		}

		public unsafe override int GetBytes (char* chars, int charCount,
			byte* bytes, int byteCount, bool flush)
		{
			int result;
			EncoderFallbackBuffer buffer = null;
			result = InternalGetBytes (chars, charCount, bytes, byteCount, Fallback, ref buffer, ref leftOverForConv, flush);
//			emitIdentifier = false;
			return result;
		}
	} // class UTF8Encoder

}; // class UTF8Encoding

}; // namespace System.Text
