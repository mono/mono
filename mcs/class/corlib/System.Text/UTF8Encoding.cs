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

	///////////////////////////////////////////////////////////////////////
	// INTERNAL DECODING FUNCTION (UTF8 -> CHAR/UTF16)
	///////////////////////////////////////////////////////////////////////

	internal enum DecoderStatus {
		Ok,
		InsufficientSpace,
		InvalidChar,
		InvalidSequence,
		InvalidStart,
		InputRunOut,
		SurrogateFound,
		Overlong,
	};

	// following method decodes an utf8 character from a byte buffer.
	// NOTE: If 'chars' is null, this function only counts bytes and chars
	//	 without writing anything.
	// NOTE: BOM (0xEF 0xBB 0xBF) is not yet supported.
	// 	 See http://www.cl.cam.ac.uk/~mgk25/unicode.html
	private unsafe static DecoderStatus InternalGetChar (
		byte* bytes, int byteCount,
		char* chars, int charCount,
		out int bytesProcessed, out int charsProcessed,
		ref uint leftBytes, ref uint leftBits, ref uint procBytes)
	{
		uint ch;
		bool checkByte;

		// reset counters
		bytesProcessed = 0;
		charsProcessed = 0;

		// Fetch the start character from the byte buffer.
		if (leftBytes == 0) {
			if (byteCount == 0)
				return DecoderStatus.InputRunOut;
			ch = (uint) (*bytes++);
			bytesProcessed++;
			byteCount--;
			procBytes = ch;
			if (ch < (uint) 0x0080) {
				// Single-byte UTF-8 character.
				leftBits = ch;
				leftBytes = 0;
			} else if (ch == (uint) 0xc0 || ch == (uint) 0xc1) {
				// invalid start
				return DecoderStatus.InvalidChar;
			} else if ((ch & (uint) 0xE0) == (uint) 0xC0) {
				// Double-byte UTF-8 character.
				leftBits = ((ch & (uint) 0x1F) << 6*1);
				leftBytes = 1;
			} else if ((ch & (uint) 0xF0) == (uint) 0xE0) {
				// Three-byte UTF-8 character.
				leftBits = ((ch & (uint) 0x0F) << 6*2);
				leftBytes = 2;
			} else if ((ch & (uint) 0xF8) == (uint) 0xF0) {
				// Four-byte UTF-8 character.
				leftBits = ((ch & (uint) 0x07) << 6*3);
				leftBytes = 3;
				// extra check for detecting as soon as
				// possible too big four-byte utf chars
				if (leftBits >= (uint) 0x110000)
					return DecoderStatus.InvalidChar;
			} else {
				// Invalid five-or-six-byte or start char
				// NOTE: I keep here the code for 5/6 bytes if
				// needed, but technically these combinations
				// are invalid in UTF-8 sequences.
				//   (ch & (uint) 0xFC) == (uint) 0xF8 =>
				//		leftBits = ch & (uint) 0x03;
				//		leftBytes = 4;
				//   (ch & (uint) 0xFE) == (uint) 0xFC =>
				//		leftBits = ch & (uint) 0x01;
				//		leftBytes = 5;
				leftBits = leftBytes = 0;
				return DecoderStatus.InvalidStart;
			}
			checkByte = (leftBytes > 0 && leftBits == 0);
		} else {
			// restore state
			checkByte = (leftBytes >> 4) != 0;
			leftBytes &= (uint) 0x0f;
		}

		// process the required bytes...
		for (; leftBytes > 0; leftBytes--) {
			if (byteCount == 0) {
				leftBytes = ((uint) (checkByte ? 0x10 : 0x00)) | leftBytes;
				return DecoderStatus.InputRunOut;
			}
			ch = (uint) (*bytes++);
			if ((ch & (uint) 0xC0) != (uint) 0x80) {
				// Invalid UTF-8 sequence: clear and restart.
				// NOTE: we return before counting the
				// 	 processed bytes for restarting
				// 	 decoding later at this point
				return DecoderStatus.InvalidSequence;
			}
			bytesProcessed++;
			byteCount--;
			procBytes = (procBytes << 8) | ch;
			if (checkByte && ((~((uint) 0x1f >> (int) leftBytes - 2)) & ch) == 0x80) {
				// detected an overlong sequence :(
				return DecoderStatus.Overlong;
			}
			checkByte = false;
			leftBits = leftBits | ((ch & (uint) 0x3F) << (6*(int) (leftBytes - 1)));
			if (leftBits >= (uint) 0x110000) {
				// this UTF-8 is too big ...
				return DecoderStatus.InvalidChar;
			}
			if ((leftBits & 0xF800) == 0xD800) {
				// UTF-8 doesn't use surrogate characters
				return DecoderStatus.SurrogateFound;
			}
		}

		// convert this character to UTF-16
		if (leftBits < (uint) 0x10000) {
			if (chars != null) {
				if (charCount < 1)
					return DecoderStatus.InsufficientSpace;
				*chars = (char) leftBits;
			}
			charsProcessed++;
		} else  {
			if (chars != null) {
				if (charCount < 2)
					return DecoderStatus.InsufficientSpace;
				leftBits -= (uint) 0x10000;
				*chars++ = (char) ((leftBits >> 10) + (uint) 0xD800);
				*chars++ = (char) ((leftBits & (uint) 0x3FF) + (uint) 0xDC00);
			}
			charsProcessed += 2;
		}

		// we've read a complete char... reset decoder status and finish
		leftBytes = leftBits = procBytes = 0;
		return DecoderStatus.Ok;
	}

	// This function is called when we want to flush the decoder state
	// (i.e. in case of invalid UTF-8 characters or interrupted sequences)
	internal unsafe static void InternalGetChars_flush (
		char* chars, int charCount,
		DecoderFallbackBuffer fallbackBuffer,
		DecoderStatus s,
		int bytesProcessed, ref int charsProcessed,
		ref uint leftBytes, ref uint leftBits, ref uint procBytes)
	{
		// now we build a 'bytesUnknown' array with the
		// stored bytes in 'procBytes'.
		int extra = 0;
		for (uint t = procBytes; t != 0; extra++)
			t = t >> 8;
		byte [] bytesUnknown = new byte [extra];
		for (int i = extra; i > 0; i--)
			bytesUnknown [i - 1] = (byte) ((procBytes >> (8 * (extra - i))) & 0xff);
		// partial reset: this condition avoids infinite loops
		if (s == DecoderStatus.InvalidSequence)
			leftBytes = 0;
		// call the fallback and cross fingers
		fallbackBuffer.Fallback (bytesUnknown, bytesProcessed - extra);
		if (chars != null) {
			while (fallbackBuffer.Remaining > 0) {
				if (charsProcessed >= charCount)
					throw new ArgumentException ("Insufficient Space", "chars/fallback");
				chars [charsProcessed++] = fallbackBuffer.GetNextChar ();
			}
		} else
			charsProcessed += fallbackBuffer.Remaining;
		fallbackBuffer.Reset ();

		// recovery was succesful, flush decoder state
		leftBits = leftBytes = procBytes = 0;
	}

	internal unsafe static DecoderStatus InternalGetChars (
		byte* bytes, int byteCount,
		char* chars, int charCount,
		DecoderFallbackBuffer fallbackBuffer,
		out int bytesProcessed, out int charsProcessed,
		ref uint leftBytes, ref uint leftBits, ref uint procBytes,
		bool flush)
	{
		DecoderStatus s;
		int t_bytesProcessed, t_charsProcessed;

		// Validate parameters
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_NonNegative"));
		else
			if (byteCount > 0 && bytes == null)
				throw new ArgumentNullException ("bytes");
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_NonNegative"));
		else
			if (charCount > 0 && chars == null)
				throw new ArgumentNullException ("chars");

		// reset counters
		charsProcessed = 0;
		bytesProcessed = 0;

		// byte processing loop
		while (byteCount - bytesProcessed > 0) {
			// fetch a char from the input byte array
			s = chars != null
				? InternalGetChar (
					bytes + bytesProcessed, byteCount - bytesProcessed,
					chars + charsProcessed, charCount - charsProcessed,
					out t_bytesProcessed, out t_charsProcessed,
					ref leftBytes, ref leftBits, ref procBytes)
				: InternalGetChar (
					bytes + bytesProcessed, byteCount - bytesProcessed,
					null, 0,
					out t_bytesProcessed, out t_charsProcessed,
					ref leftBytes, ref leftBits, ref procBytes);

			// if not enough space return here
			// NOTE: maybe we should restore the original encoder
			//       state ... we should check what ms do in this case
			if(s == DecoderStatus.InsufficientSpace)
				return DecoderStatus.InsufficientSpace;

			// update counters
			charsProcessed += t_charsProcessed;
			bytesProcessed += t_bytesProcessed;

			switch (s) {
			case DecoderStatus.Ok:
				break;	// everything OK :D

			case DecoderStatus.Overlong:
			case DecoderStatus.InvalidSequence:
			case DecoderStatus.InvalidStart:
			case DecoderStatus.InvalidChar:
			case DecoderStatus.SurrogateFound:
				InternalGetChars_flush (
					chars, charCount,
					fallbackBuffer,
					s,
					bytesProcessed, ref charsProcessed,
					ref leftBytes, ref leftBits, ref procBytes);
				break;

			case DecoderStatus.InputRunOut:
				if (flush)
					InternalGetChars_flush (
						chars, charCount,
						fallbackBuffer,
						s,
						bytesProcessed, ref charsProcessed,
						ref leftBytes, ref leftBits, ref procBytes);
				return DecoderStatus.InputRunOut;
			}
		}
		return DecoderStatus.Ok;
	}

	// Get the characters that result from decoding a byte buffer.
	internal unsafe static DecoderStatus InternalGetChars (
		byte[] bytes, int byteIndex, int byteCount,
		char[] chars, int charIndex,
		DecoderFallbackBuffer fallbackBuffer,
		out int bytesProcessed, out int charsProcessed,
		ref uint leftBytes, ref uint leftBits, ref uint procBytes,
		bool flush)
	{
		// Validate the parameters.
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (byteIndex < 0 || byteIndex >= bytes.Length)
			throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));
		if (byteCount < 0 || byteCount > (bytes.Length - byteIndex))
			throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_Array"));
		if (charIndex < 0 || charIndex > (chars != null && chars.Length > 0 ? chars.Length : 0))
			throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_Array"));

		fixed (char* cptr = chars) {
			fixed (byte* bptr = bytes) {
				return InternalGetChars (
						bptr + byteIndex, byteCount,
						chars != null ? cptr + charIndex : null,
						chars != null ? chars.Length - charIndex : 0,
						fallbackBuffer,
						out bytesProcessed, out charsProcessed,
						ref leftBytes, ref leftBits, ref procBytes,
						flush);
			}
		}
	}

	///////////////////////////////////////////////////////////////////////
	// INTERNAL ENCODING FUNCTION (CHAR/UTF16 -> UTF8)
	///////////////////////////////////////////////////////////////////////

	internal enum EncoderStatus {
		Ok,
		InputRunOut,
		InsufficientSpace,
		InvalidChar,
		InvalidSurrogate,
	};

	// following method encodes an utf8 character into a byte buffer.
	// NOTE: If 'byteCount' is < 0, this function only counts used bytes
	//       without writing anything.
	// NOTE: BOM (0xEF 0xBB 0xBF) is not yet supported.
	// 	 See http://www.cl.cam.ac.uk/~mgk25/unicode.html
	private unsafe static EncoderStatus InternalGetByte (
		char* chars, int charCount,
		byte* bytes, int byteCount,
		out int charsProcessed, out int bytesProcessed, ref uint leftChar)
	{
		uint ch;

		// reset counters
		charsProcessed = 0;
		bytesProcessed = 0;

		// process one char (this block executes twice if a surrogate is found)
again:
		if (charCount < 1)
			return EncoderStatus.InputRunOut;

		ch = *chars++;

		if (leftChar == 0) {
			// char counting is inside if for reason discused in else
			charsProcessed++;
			charCount--;
			if (ch < (uint) 0x80) {
				if (byteCount >= 0) {
					if (byteCount < 1)
						return EncoderStatus.InsufficientSpace;
					*bytes++ = (byte) ch;
					byteCount--;
				}
				bytesProcessed++;
			} else if (ch < (uint) 0x0800) {
				if (byteCount >= 0) {
					if (byteCount < 2)
						return EncoderStatus.InsufficientSpace;
					*bytes++ = (byte) ((uint) 0xC0 | (ch >> 6) & 0x3f);
					*bytes++ = (byte) ((uint) 0x80 | ch & 0x3f);
					byteCount -= 2;
				}
				bytesProcessed += 2;
			} else if (ch < (uint) 0xD800 || ch > (uint) 0xDFFF) {
				if (byteCount >= 0) {
					if (byteCount < 3)
						return EncoderStatus.InsufficientSpace;
					*bytes++ = (byte) ((uint) 0xE0 | (ch >> 12));
					*bytes++ = (byte) ((uint) 0x80 | ((ch >> 6) & 0x3F));
					*bytes++ = (byte) ((uint) 0x80 | (ch & 0x3F));
					byteCount -= 3;
				}
				bytesProcessed += 3;
			} else if (ch <= (uint) 0xDBFF) {
				// This is a surrogate char, repeat please
				leftChar = ch;
				goto again;
			} else {
				// We have a surrogate tail without 
				// leading surrogate.
				leftChar = ch;
				return EncoderStatus.InvalidChar;
			}
		} else {
			if (ch >= (uint) 0xDC00 && ch <= (uint) 0xDFFF) {
				// We have a correct surrogate pair.
				ch = 0x10000 + (uint) ch - (uint) 0xDC00
					+ ((leftChar - (uint) 0xD800) << 10);
				if (byteCount >= 0) {
					if (byteCount < 4)
						return EncoderStatus.InsufficientSpace;
					*bytes++ = (byte) (0xF0 | (ch >> 18));
					*bytes++ = (byte) (0x80 | ((ch >> 12) & 0x3F));
					*bytes++ = (byte) (0x80 | ((ch >> 6) & 0x3F));
					*bytes++ = (byte) (0x80 | (ch & 0x3F));
					byteCount -= 4;
				}
				bytesProcessed += 4;
			} else {
				// We have a surrogate start followed by a
				// regular character.  Technically, this is
				// invalid, so we fail :(
				return EncoderStatus.InvalidSurrogate;
			}
			// increment counters; this is done after processing
			// the surrogate: in case of a bad surrogate the
			// encoding should restart on the faulty char (maybe
			// the correct surrogate has been lost, and in this
			// case the best option is to restart processing on the
			// erroneus char to avoid losing more chars during the
			// encoding.
			charsProcessed++;
			charCount--;
			leftChar = 0;
		}
		return EncoderStatus.Ok;
	}

	// This function is called when we want to flush the decoder state
	// (i.e. in case of invalid UTF-16 characters or dangling surrogates)
	internal unsafe static void InternalGetBytesFlush (
		byte* bytes, int byteCount,
		EncoderFallbackBuffer fallbackBuffer,
		int charsProcessed, ref int bytesProcessed,
		ref uint leftChar)
	{
		int t_charsProcessed, t_bytesProcessed;

		// in normal circumstances fallbackBuffer never is null, except
		// when we have called InternalGetBytes from this function
		// (for avoiding infinite recursive calls)
		if (fallbackBuffer == null)
			return;

		// invalid UTF-16 or invalid surrogate
		fallbackBuffer.Fallback ((char) leftChar, charsProcessed - 1);
		// if we've arrived here we are working in replacement mode:
		// build a replacement fallback_chars buffer
		char[] fallback_chars = new char [fallbackBuffer.Remaining];
		for (int i = 0; i < fallback_chars.Length; i++)
			fallback_chars [i] = fallbackBuffer.GetNextChar ();
		fallbackBuffer.Reset ();
		// and encode it into UTF8 bytes...
		fixed (char *fb_chars = fallback_chars) {
			leftChar = 0;
			switch (bytes != null
				? InternalGetBytes (
						fb_chars, fallback_chars.Length,
						bytes + bytesProcessed, byteCount - bytesProcessed,
						null, out t_charsProcessed, out t_bytesProcessed,
						ref leftChar,
						true)
				: InternalGetBytes (
						fb_chars, fallback_chars.Length,
						null, byteCount,
						null, out t_charsProcessed, out t_bytesProcessed,
						ref leftChar,
						true)) {
			case EncoderStatus.Ok:
				// everything OK :D
				bytesProcessed += t_bytesProcessed;
				break;
			case EncoderStatus.InsufficientSpace:
				throw new ArgumentException ("Insufficient Space", "fallback buffer bytes");
			case EncoderStatus.InputRunOut:
			case EncoderStatus.InvalidChar:
			case EncoderStatus.InvalidSurrogate:
				throw new ArgumentException ("Fallback chars are pure evil.", "fallback buffer bytes");
			}
		}
		// flush encoder state
		leftChar = 0;
	}

	// InternalGetBytes processor. Can encode or count space needed for
	// encoding, depending on the enabled mode:
	//   - encoder
	//       enabled when byteCount >= 0 (but bytes may be null)
	//   - counter
	//       enabled when bytes == null && byteCount < 0
	internal unsafe static EncoderStatus InternalGetBytes (
		char* chars, int charCount,
		byte* bytes, int byteCount,
		EncoderFallbackBuffer fallbackBuffer,
		out int charsProcessed, out int bytesProcessed,
		ref uint leftChar,
		bool flush)
	{
		EncoderStatus s;
		int t_charsProcessed, t_bytesProcessed;

		// Validate the parameters
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_NonNegative"));
		else
			if (charCount > 0 && chars == null)
				throw new ArgumentNullException ("chars");
 		if (bytes == null)
 		{
 			if (byteCount > 0)
				throw new ArgumentNullException ("bytes");
		} else {
			if (byteCount <= 0)
				throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_NonNegative"));
		}

		// reset counters
		charsProcessed = 0;
		bytesProcessed = 0;

		// char processing loop
		while (charCount - charsProcessed > 0) {
			s = bytes != null
				? InternalGetByte (
					chars + charsProcessed, charCount - charsProcessed,
					bytes + bytesProcessed, byteCount - bytesProcessed,
					out t_charsProcessed, out t_bytesProcessed, ref leftChar)
				: InternalGetByte (
					chars + charsProcessed, charCount - charsProcessed,
					null, byteCount,
					out t_charsProcessed, out t_bytesProcessed, ref leftChar);

			// if not enough space return here
			// NOTE: maybe we should restore the original encoder
			//       state ... we should check what ms do in this case
			if(s == EncoderStatus.InsufficientSpace)
				return EncoderStatus.InsufficientSpace;

			// update counters
			charsProcessed += t_charsProcessed;
			bytesProcessed += t_bytesProcessed;

			switch (s) {
			case EncoderStatus.Ok:
				break;	// everything OK :D

			case EncoderStatus.InputRunOut:
				if (flush)
					InternalGetBytesFlush (
						bytes, byteCount,
						fallbackBuffer,
						charsProcessed, ref bytesProcessed,
						ref leftChar);
				return EncoderStatus.InputRunOut;

			case EncoderStatus.InvalidChar:
			case EncoderStatus.InvalidSurrogate:
				InternalGetBytesFlush (
					bytes, byteCount,
					fallbackBuffer,
					charsProcessed, ref bytesProcessed,
					ref leftChar);
				break;
			}
		}
		return EncoderStatus.Ok;
	}

	internal unsafe static EncoderStatus InternalGetBytesEncode (
		char* chars, int charCount,
		byte* bytes, int byteCount,
		EncoderFallbackBuffer fallbackBuffer,
		out int charsProcessed, out int bytesProcessed,
		ref uint leftChar,
		bool flush)
	{
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_Array"));
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_Array"));

		return InternalGetBytes (
			chars, charCount,
			bytes, byteCount,
			fallbackBuffer,
			out charsProcessed, out bytesProcessed,
			ref leftChar,
			flush);
	}

	internal unsafe static EncoderStatus InternalGetBytesEncode (
		char[] chars, int charIndex, int charCount,
		byte[] bytes, int byteIndex,
		EncoderFallbackBuffer fallbackBuffer,
		out int charsProcessed, out int bytesProcessed,
		ref uint leftChar,
		bool flush)
	{
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (charIndex < 0 || charIndex >= chars.Length)
			throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_Array"));
		if (charCount < 0 || charCount > (chars.Length - charIndex))
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_Array"));
		if (byteIndex < 0 || byteIndex > bytes.Length)
			throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));

		unsafe {
			fixed (char *cptr = chars) {
				fixed (byte *bptr = bytes) {
					return InternalGetBytes (
						cptr + charIndex, charCount,
						bptr + byteIndex, bytes.Length - byteIndex,
						fallbackBuffer,
						out charsProcessed, out bytesProcessed,
						ref leftChar,
						flush);
				}
			}
		}
	}

	internal unsafe static EncoderStatus InternalGetBytesCount (
		char* chars, int charCount,
		EncoderFallbackBuffer fallbackBuffer,
		out int charsProcessed, out int bytesProcessed,
		ref uint leftChar,
		bool flush)
	{
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_Array"));

		return InternalGetBytes (
			chars, charCount,
			null, -1,
			fallbackBuffer,
			out charsProcessed, out bytesProcessed,
			ref leftChar,
			flush);
	}

	internal unsafe static EncoderStatus InternalGetBytesCount (
		char[] chars, int charIndex, int charCount,
		EncoderFallbackBuffer fallbackBuffer,
		out int charsProcessed, out int bytesProcessed,
		ref uint leftChar,
		bool flush)
	{
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (charIndex < 0 || charIndex >= chars.Length)
			throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_Array"));
		if (charCount < 0 || charCount > (chars.Length - charIndex))
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_Array"));

		unsafe {
			fixed (char *cptr = chars) {
				return InternalGetBytes (
					cptr + charIndex, charCount,
					null, -1,
					fallbackBuffer,
					out charsProcessed, out bytesProcessed,
					ref leftChar,
					flush);
			}
		}
	}

	#region GetByteCount()

	// Get the number of bytes needed to encode a character buffer.
	public override int GetByteCount (char[] chars, int index, int count)
	{
		uint leftChar = 0;
		int charsProcessed, bytesProcessed;
		InternalGetBytesCount (
				chars, index, count,
				EncoderFallback.CreateFallbackBuffer (),
				out charsProcessed, out bytesProcessed,
				ref leftChar,
				true);
		return bytesProcessed;
	}


	[CLSCompliant (false)]
	[ComVisible (false)]
	public unsafe override int GetByteCount (char* chars, int count)
	{
		int charsProcessed, bytesProcessed;
		uint leftChar = 0;
		InternalGetBytesCount (
			chars, count,
			EncoderFallback.CreateFallbackBuffer (),
			out charsProcessed, out bytesProcessed,
			ref leftChar,
			true);
		return bytesProcessed;
	}

	#endregion

	#region GetBytes()

	// Get the bytes that result from encoding a character buffer.
	public override int GetBytes (char[] chars, int charIndex, int charCount,
				      byte[] bytes, int byteIndex)
	{
		int charsProcessed, bytesProcessed;
		uint leftChar = 0;
		if (InternalGetBytesEncode (
				chars, charIndex, charCount,
				bytes, byteIndex,
				EncoderFallback.CreateFallbackBuffer (),
				out charsProcessed, out bytesProcessed,
				ref leftChar,
				true) == EncoderStatus.InsufficientSpace)
			throw new ArgumentException ("Insufficient Space", "bytes");
		return bytesProcessed;
	}

	// Convenience wrappers for "GetBytes".
	public unsafe override int GetBytes (String s, int charIndex, int charCount,
				      byte[] bytes, int byteIndex)
	{
		int charsProcessed, bytesProcessed;
		uint leftChar = 0;
		EncoderStatus status;
		if (s == null)
			throw new ArgumentNullException ("s");
		if (charIndex < 0 || charIndex >= s.Length)
			throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_StringIndex"));
		if (charCount < 0 || charCount > (s.Length - charIndex))
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_StringRange"));
		if (byteIndex < 0 || byteIndex > bytes.Length)
			throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));
		unsafe {
			fixed (char *cptr = s) {
				fixed (byte *bptr = bytes) {
					status = InternalGetBytesEncode (
						cptr + charIndex, charCount,
						bptr + byteIndex, bytes.Length - byteIndex,
						EncoderFallback.CreateFallbackBuffer (),
						out charsProcessed, out bytesProcessed,
						ref leftChar,
						true);
				}
			}
		}
		if (status == EncoderStatus.InsufficientSpace)
			throw new ArgumentException ("Insufficient Space", "bytes");
		return bytesProcessed;
	}

	[CLSCompliant (false)]
	[ComVisible (false)]
	public unsafe override int GetBytes (char* chars, int charCount, byte* bytes, int byteCount)
	{
		int charsProcessed, bytesProcessed;
		uint leftChar = 0;
		if (InternalGetBytesEncode (
				chars, charCount, bytes, byteCount,
				EncoderFallback.CreateFallbackBuffer (),
				out charsProcessed, out bytesProcessed,
				ref leftChar,
				true) == EncoderStatus.InsufficientSpace)
			throw new ArgumentException ("Insufficient Space", "bytes");
		return bytesProcessed;
	}

	#endregion

	#region GetCharCount()

	// Get the number of characters needed to decode a byte buffer.
	public override int GetCharCount (byte[] bytes, int index, int count)
	{
		int bytesProcessed, charsProcessed;
		uint leftBytes = 0, leftBits = 0, procBytes = 0;
		InternalGetChars (
			bytes, index, count,
			null, 0,
			DecoderFallback.CreateFallbackBuffer (),
			out bytesProcessed, out charsProcessed,
			ref leftBytes, ref leftBits, ref procBytes,
			true);
		return charsProcessed;
	}

	[CLSCompliant (false)]
	[ComVisible (false)]
	public unsafe override int GetCharCount (byte* bytes, int count)
	{
		int bytesProcessed, charsProcessed;
		uint leftBytes = 0, leftBits = 0, procBytes = 0;
		InternalGetChars (
			bytes, count,
			null, 0,
			DecoderFallback.CreateFallbackBuffer (),
			out bytesProcessed, out charsProcessed,
			ref leftBytes, ref leftBits, ref procBytes,
			true);
		return charsProcessed;
	}

	#endregion

	// Get the characters that result from decoding a byte buffer.
	public override int GetChars (byte[] bytes, int byteIndex, int byteCount,
				      char[] chars, int charIndex)
	{
		int bytesProcessed, charsProcessed;
		uint leftBytes = 0, leftBits = 0, procBytes = 0;
		DecoderStatus status;

		status = InternalGetChars (
			bytes, byteIndex, byteCount,
			chars, charIndex,
			DecoderFallback.CreateFallbackBuffer (),
			out bytesProcessed, out charsProcessed,
			ref leftBytes, ref leftBits, ref procBytes,
			true);
		if (status == DecoderStatus.InsufficientSpace)
			throw new ArgumentException ("Insufficient Space", "bytes");

		return charsProcessed;
	}

	[CLSCompliant (false)]
	[ComVisible (false)]
	public unsafe override int GetChars (byte* bytes, int byteCount, char* chars, int charCount)
	{
		int bytesProcessed, charsProcessed;
		uint leftBytes = 0, leftBits = 0, procBytes = 0;
		DecoderStatus status;

		status = InternalGetChars (
			bytes, byteCount,
			chars, charCount,
			DecoderFallback.CreateFallbackBuffer (),
			out bytesProcessed, out charsProcessed,
			ref leftBytes, ref leftBits, ref procBytes,
			true);
		if (status == DecoderStatus.InsufficientSpace)
			throw new ArgumentException ("Insufficient Space", "bytes");

		return charsProcessed;
	}

	// Get the maximum number of bytes needed to encode a
	// specified number of characters.
	public override int GetMaxByteCount (int charCount)
	{
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_NonNegative"));
		return charCount * 4;
	}

	// Get the maximum number of characters needed to decode a
	// specified number of bytes.
	public override int GetMaxCharCount (int byteCount)
	{
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_NonNegative"));
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
	// XXX: why does this method return a preamble or void array depending
	//      on 'emitIdentifier' attribute?
	public override byte[] GetPreamble ()
	{
		if (emitIdentifier)
			return new byte [] { 0xEF, 0xBB, 0xBF };

		return EmptyArray<byte>.Value;
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
		// internal encoder state
		private uint leftBytes;
		private uint leftBits;
		private uint procBytes;

		// Constructor.
		public UTF8Decoder (DecoderFallback fallback)
		{
			Fallback = fallback;
			leftBytes = 0;
			leftBits = 0;
			procBytes = 0;
		}

		// Override inherited methods.
		public override int GetCharCount (byte[] bytes, int index, int count, bool flush)
		{
			int bytesProcessed, charsProcessed;
			InternalGetChars (
				bytes, index, count,
				null, 0,
				this.FallbackBuffer,
				out bytesProcessed, out charsProcessed,
				ref leftBytes, ref leftBits, ref procBytes,
				flush);
			return charsProcessed;
		}

		[ComVisibleAttribute(false)]
		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			return GetCharCount (bytes, index, count, true);
		}

		[ComVisibleAttribute(false)] 
		public unsafe override int GetCharCount (byte* bytes, int count, bool flush)
		{
			int bytesProcessed, charsProcessed;
			InternalGetChars (
				bytes, count,
				null, 0,
				this.FallbackBuffer,
				out bytesProcessed, out charsProcessed,
				ref leftBytes, ref leftBits, ref procBytes,
				flush);
			return charsProcessed;
		}

		[ComVisibleAttribute(false)]
		public unsafe override int GetChars (byte* bytes, int byteCount,
						char* chars, int charCount, bool flush)
		{
			int bytesProcessed, charsProcessed;
			DecoderStatus status;

			status = InternalGetChars (
				bytes, byteCount,
				chars, charCount,
				this.FallbackBuffer,
				out bytesProcessed, out charsProcessed,
				ref leftBytes, ref leftBits, ref procBytes,
				flush);
			if (status == DecoderStatus.InsufficientSpace)
				throw new ArgumentException ("Insufficient Space", "bytes");
			return charsProcessed;
		}

		public override int GetChars (byte[] bytes, int byteIndex,
						 int byteCount, char[] chars, int charIndex, bool flush)
		{
			int bytesProcessed, charsProcessed;
			DecoderStatus status;

			status = InternalGetChars (
				bytes, byteIndex, byteCount,
				chars, charIndex,
				this.FallbackBuffer,
				out bytesProcessed, out charsProcessed,
				ref leftBytes, ref leftBits, ref procBytes,
				flush);
			if (status == DecoderStatus.InsufficientSpace)
				throw new ArgumentException ("Insufficient Space", "bytes");
			return charsProcessed;
		}

		public override int GetChars (byte[] bytes, int byteIndex,
						 int byteCount, char[] chars, int charIndex)
		{
			return GetChars (bytes, byteIndex, byteCount, chars, charIndex, true);
		}

		public override void Reset ()
		{
			base.Reset ();
			leftBytes = 0;
			leftBits = 0;
			procBytes = 0;
		}

		public unsafe override void Convert (
			byte* bytes, int byteCount,
			char* chars, int charCount, bool flush,
			out int bytesUsed, out int charsUsed, out bool completed)
		{
			UTF8Encoding.InternalGetChars (
					bytes, byteCount,
					chars, charCount,
					this.FallbackBuffer,
					out bytesUsed, out charsUsed,
					ref leftBytes, ref leftBits, ref procBytes,
					flush);
			// only completed if all bytes have been processed and
			// succesful converted to chars!!
			completed = (byteCount == bytesUsed);
		}
	} // class UTF8Decoder

	// UTF-8 encoder implementation.
	[Serializable]
	private class UTF8Encoder : Encoder
	{
		private bool emitIdentifier;

		// internal encoder state
		private uint leftChar;
		private bool emittedIdentifier;

		// Constructor.
		public UTF8Encoder (EncoderFallback fallback, bool emitIdentifier)
		{
			this.Fallback = fallback;
			this.leftChar = 0;
			this.emitIdentifier = emitIdentifier;
			this.emittedIdentifier = false;
		}

		// Override inherited methods.
		[ComVisibleAttribute(false)]
		public unsafe override int GetByteCount (char* chars, int count, bool flush)
		{
			int charsProcessed, bytesProcessed, preambleSize = 0;
			if (emitIdentifier && !emittedIdentifier) {
				preambleSize = 3;
				emittedIdentifier = true;
			}
			InternalGetBytesCount (
					chars, count,
					this.FallbackBuffer,
					out charsProcessed, out bytesProcessed,
					ref leftChar,
					flush);
			return bytesProcessed + preambleSize;
		}

		public override int GetByteCount (char[] chars, int index,
							int count, bool flush)
		{
			int charsProcessed, bytesProcessed, preambleSize = 0;
			if (emitIdentifier && !emittedIdentifier) {
				preambleSize = 3;
				emittedIdentifier = true;
			}
			InternalGetBytesCount (
					chars, index, count,
					this.FallbackBuffer,
					out charsProcessed, out bytesProcessed,
					ref leftChar,
					flush);
			return bytesProcessed + preambleSize;
		}

		[ComVisibleAttribute(false)]
		public unsafe override int GetBytes (char* chars, int charCount,
			byte* bytes, int byteCount, bool flush)
		{
			int charsProcessed, bytesProcessed, preambleSize = 0;
			if (emitIdentifier && !emittedIdentifier) {
				if (byteCount < 3)
					throw new ArgumentException ("Insufficient Space", "UTF8 preamble");
				*bytes++ = 0xEF;
				*bytes++ = 0xBB;
				*bytes++ = 0xBF;
				preambleSize = 3;
				emittedIdentifier = true;
				byteCount -= 3;
			}
			if (InternalGetBytesEncode (
					chars, charCount,
					bytes, byteCount,
					this.FallbackBuffer,
					out charsProcessed, out bytesProcessed,
					ref leftChar,
					flush) == EncoderStatus.InsufficientSpace)
				throw new ArgumentException ("Insufficient Space", "bytes");
			return bytesProcessed + preambleSize;
		}

		public override int GetBytes (char[] chars, int charIndex,
						int charCount, byte[] bytes,
						int byteIndex, bool flush)
		{
			int charsProcessed, bytesProcessed, preambleSize = 0;
			if (emitIdentifier && !emittedIdentifier) {
				if (bytes.Length - byteIndex < 3)
					throw new ArgumentException ("Insufficient Space", "UTF8 preamble");
				bytes[byteIndex++] = 0xEF;
				bytes[byteIndex++] = 0xBB;
				bytes[byteIndex++] = 0xBF;
				preambleSize = 3;
				emittedIdentifier = true;
			}
			if (InternalGetBytesEncode (
					chars, charIndex, charCount,
					bytes, byteIndex,
					this.FallbackBuffer,
					out charsProcessed, out bytesProcessed,
					ref leftChar,
					flush) == EncoderStatus.InsufficientSpace)
				throw new ArgumentException ("Insufficient Space", "bytes");
			return bytesProcessed + preambleSize;
		}

		public override void Reset ()
		{
			base.Reset ();
			this.leftChar = 0;
			this.emittedIdentifier = false;
		}

		public unsafe override void Convert (
			char* chars, int charCount,
			byte* bytes, int byteCount, bool flush,
			out int charsUsed, out int bytesUsed, out bool completed)
		{
			int preambleSize = 0;
			if (emitIdentifier && !emittedIdentifier) {
				if (bytes != null && byteCount >= 3)
				{
					*bytes++ = 0xEF;
					*bytes++ = 0xBB;
					*bytes++ = 0xBF;
					preambleSize = 3;
					emittedIdentifier = true;
					byteCount -= 3;
				}
			}
			InternalGetBytesEncode (
					chars, charCount,
					bytes, byteCount,
					this.FallbackBuffer,
					out charsUsed, out bytesUsed,
					ref leftChar,
					flush);
			// only completed if all chars have been processed and
			// succesful converted to chars!!
			completed = (charCount == charsUsed);
			bytesUsed += preambleSize;
		}
	} // class UTF8Encoder

}; // class UTF8Encoding

}; // namespace System.Text
