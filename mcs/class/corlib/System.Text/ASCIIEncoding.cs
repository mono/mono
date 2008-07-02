/*
 * ASCIIEncoding.cs - Implementation of the "System.Text.ASCIIEncoding" class.
 *
 * Copyright (c) 2001  Southern Storm Software, Pty Ltd
 * Copyright (C) 2003 Novell, Inc.
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
#if NET_2_0
[ComVisible (true)]
#endif
[MonoTODO ("Serialization format not compatible with .NET")]
public class ASCIIEncoding : Encoding
{
	// Magic number used by Windows for "ASCII".
	internal const int ASCII_CODE_PAGE = 20127;

	// Constructor.
	public ASCIIEncoding () : base(ASCII_CODE_PAGE) {
		body_name = header_name = web_name= "us-ascii";
		encoding_name = "US-ASCII";
		is_mail_news_display = true;
		is_mail_news_save = true;
	}

#if NET_2_0
	[ComVisible (false)]
	public override bool IsSingleByte {
		get { return true; }
	}
#endif

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
		return count;
	}

	// Convenience wrappers for "GetByteCount".
	public override int GetByteCount (String chars)
	{
		if (chars == null) {
			throw new ArgumentNullException ("chars");
		}
		return chars.Length;
	}

	// Get the bytes that result from encoding a character buffer.
	public override int GetBytes (char[] chars, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
	{
#if NET_2_0
// well, yes, I know this #if is ugly, but I think it is the simplest switch.
		EncoderFallbackBuffer buffer = null;
		char [] fallback_chars = null;
		return GetBytes (chars, charIndex, charCount, bytes,
			byteIndex, ref buffer, ref fallback_chars);
	}

	int GetBytes (char[] chars, int charIndex, int charCount,
		      byte[] bytes, int byteIndex,
		      ref EncoderFallbackBuffer buffer,
		      ref char [] fallback_chars)
	{
#endif
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
		if ((bytes.Length - byteIndex) < charCount) {
			throw new ArgumentException (_("Arg_InsufficientSpace"));
		}
		int count = charCount;
		char ch;
		while (count-- > 0) {
			ch = chars [charIndex++];
			if (ch < (char)0x80) {
				bytes [byteIndex++] = (byte)ch;
			} else {
#if NET_2_0
				if (buffer == null)
					buffer = EncoderFallback.CreateFallbackBuffer ();
				if (Char.IsSurrogate (ch) && count > 1 &&
				    Char.IsSurrogate (chars [charIndex]))
					buffer.Fallback (ch, chars [charIndex], charIndex++ - 1);
				else
					buffer.Fallback (ch, charIndex - 1);
				if (fallback_chars == null || fallback_chars.Length < buffer.Remaining)
					fallback_chars = new char [buffer.Remaining];
				for (int i = 0; i < fallback_chars.Length; i++)
					fallback_chars [i] = buffer.GetNextChar ();
				byteIndex += GetBytes (fallback_chars, 0, 
					fallback_chars.Length, bytes, byteIndex,
					ref buffer, ref fallback_chars);
#else
				bytes [byteIndex++] = (byte)'?';
#endif
			}
		}
		return charCount;
	}

	// Convenience wrappers for "GetBytes".
	public override int GetBytes (String chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
#if NET_2_0
// I know this #if is ugly, but I think it is the simplest switch.
		EncoderFallbackBuffer buffer = null;
		char [] fallback_chars = null;
		return GetBytes (chars, charIndex, charCount, bytes, byteIndex,
			ref buffer, ref fallback_chars);
	}

	int GetBytes (String chars, int charIndex, int charCount,
		      byte[] bytes, int byteIndex,
		      ref EncoderFallbackBuffer buffer,
		      ref char [] fallback_chars)
	{
#endif
		if (chars == null) {
			throw new ArgumentNullException ("chars");
		}
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		if (charIndex < 0 || charIndex > chars.Length) {
			throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_StringIndex"));
		}
		if (charCount < 0 || charCount > (chars.Length - charIndex)) {
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_StringRange"));
		}
		if (byteIndex < 0 || byteIndex > bytes.Length) {
			throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));
		}
		if ((bytes.Length - byteIndex) < charCount) {
			throw new ArgumentException (_("Arg_InsufficientSpace"));
		}
		int count = charCount;
		char ch;
		while (count-- > 0) {
			ch = chars [charIndex++];
			if (ch < (char)0x80) {
				bytes [byteIndex++] = (byte)ch;
			} else {
#if NET_2_0
				if (buffer == null)
					buffer = EncoderFallback.CreateFallbackBuffer ();
				if (Char.IsSurrogate (ch) && count > 1 &&
				    Char.IsSurrogate (chars [charIndex]))
					buffer.Fallback (ch, chars [charIndex], charIndex++ - 1);
				else
					buffer.Fallback (ch, charIndex - 1);
				if (fallback_chars == null || fallback_chars.Length < buffer.Remaining)
					fallback_chars = new char [buffer.Remaining];
				for (int i = 0; i < fallback_chars.Length; i++)
					fallback_chars [i] = buffer.GetNextChar ();
				byteIndex += GetBytes (fallback_chars, 0, 
					fallback_chars.Length, bytes, byteIndex,
					ref buffer, ref fallback_chars);
#else
				bytes [byteIndex++] = (byte)'?';
#endif
			}
		}
		return charCount;
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
		return count;
	}

	// Get the characters that result from decoding a byte buffer.
	public override int GetChars (byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
#if NET_2_0
// well, yes, I know this #if is ugly, but I think it is the simplest switch.
		DecoderFallbackBuffer buffer = null;
		return GetChars (bytes, byteIndex, byteCount, chars,
			charIndex, ref buffer);
	}

	int GetChars (byte[] bytes, int byteIndex, int byteCount,
		      char[] chars, int charIndex,
		      ref DecoderFallbackBuffer buffer)
	{
#endif
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (chars == null) 
			throw new ArgumentNullException ("chars");
		if (byteIndex < 0 || byteIndex > bytes.Length) 
			throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));
		if (byteCount < 0 || byteCount > (bytes.Length - byteIndex)) 
			throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_Array"));
		if (charIndex < 0 || charIndex > chars.Length) 
			throw new ArgumentOutOfRangeException ("charIndex", _("ArgRange_Array"));

		if ((chars.Length - charIndex) < byteCount) 
			throw new ArgumentException (_("Arg_InsufficientSpace"));

		int count = byteCount;
		while (count-- > 0) {
			char c = (char) bytes [byteIndex++];
			if (c < '\x80')
				chars [charIndex++] = c;
			else {
#if NET_2_0
				if (buffer == null)
					buffer = DecoderFallback.CreateFallbackBuffer ();
				buffer.Fallback (bytes, byteIndex);
				while (buffer.Remaining > 0)
					chars [charIndex++] = buffer.GetNextChar ();
#else
				chars [charIndex++] = '?';
#endif
			}
		}
		return byteCount;
	}

	// Get the maximum number of bytes needed to encode a
	// specified number of characters.
	public override int GetMaxByteCount (int charCount)
	{
		if (charCount < 0) {
			throw new ArgumentOutOfRangeException ("charCount", _("ArgRange_NonNegative"));
		}
		return charCount;
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

	// Decode a buffer of bytes into a string.
	public override String GetString (byte[] bytes, int byteIndex, int byteCount)
	{
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		if (byteIndex < 0 || byteIndex > bytes.Length) {
			throw new ArgumentOutOfRangeException ("byteIndex", _("ArgRange_Array"));
		}
		if (byteCount < 0 || byteCount > (bytes.Length - byteIndex)) {
			throw new ArgumentOutOfRangeException ("byteCount", _("ArgRange_Array"));
		}
		if (byteCount == 0)
			return String.Empty;
		
		unsafe {
			fixed (byte* bytePtr = bytes) {
				string s = string.InternalAllocateStr (byteCount);

				fixed (char* charPtr = s) {
					byte* currByte = bytePtr + byteIndex;
					byte* lastByte = currByte + byteCount;
					char* currChar = charPtr;

					while (currByte < lastByte) {
#if NET_2_0
						byte b = currByte++ [0];
						currChar++ [0] = b <= 0x7F ? (char) b : (char) '?';
#else
						// GetString is incompatible with GetChars
						currChar++ [0] = (char) (currByte++ [0] & 0x7F);
#endif
					}
				}

				return s;
			}
		}
	}

#if NET_2_0
	[CLSCompliantAttribute (false)]
	[ComVisible (false)]
	public unsafe override int GetBytes (char *chars, int charCount, byte *bytes, int byteCount)
	{
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount");
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount");

		if (byteCount < charCount)
			throw new ArgumentException ("bytecount is less than the number of bytes required", "byteCount");

		for (int i = 0; i < charCount; i++){
			char c = chars [i];
			bytes [i] = (byte) ((c < (char) 0x80) ? c : '?');
		}
		return charCount;
	}

	[CLSCompliantAttribute(false)]
	[ComVisible (false)]
	public unsafe override int GetChars (byte *bytes, int byteCount, char *chars, int charCount)
	{
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (chars == null) 
			throw new ArgumentNullException ("chars");
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount");
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount");
		if (charCount < byteCount)
			throw new ArgumentException ("charcount is less than the number of bytes required", "charCount");

		for (int i = 0; i < byteCount; i++){
			byte b = bytes [i];
			chars [i] = b > 127 ? '?' : (char) b;
		}
		return byteCount;
		
	}

	[CLSCompliantAttribute(false)]
	[ComVisible (false)]
	public unsafe override int GetCharCount (byte *bytes, int count)
	{
		return count;
	}

	[CLSCompliantAttribute(false)]
	[ComVisible (false)]
	public unsafe override int GetByteCount (char *chars, int count)
	{
		return count;
	}
#else
	// This routine is gone in 2.x
	public override String GetString (byte[] bytes)
	{
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}

		return GetString (bytes, 0, bytes.Length);
	}
#endif

#if NET_2_0
	[MonoTODO ("we have simple override to match method signature.")]
	[ComVisible (false)]
	public override Decoder GetDecoder ()
	{
		return base.GetDecoder ();
	}

	[MonoTODO ("we have simple override to match method signature.")]
	[ComVisible (false)]
	public override Encoder GetEncoder ()
	{
		return base.GetEncoder ();
	}
#endif
}; // class ASCIIEncoding

}; // namespace System.Text
