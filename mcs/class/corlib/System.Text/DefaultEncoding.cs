/*
 * DefaultEncoding.cs - Implementation of the
 *		"System.Text.DefaultEncoding" class.
 *
 * Copyright (c) 2001  Southern Storm Software, Pty Ltd
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
using System.Runtime.CompilerServices;

internal sealed class DefaultEncoding : Encoding
{
	// Constructor.
	public DefaultEncoding () : base(0) {}

	// Get the number of bytes needed to encode a character buffer.
	unsafe public override int GetByteCount (char[] chars, int index, int count)
	{
		if (chars == null)
		{
			throw new ArgumentNullException ("chars");
		}
		if (index < 0 || index > chars.Length)
		{
			throw new ArgumentOutOfRangeException
				("index", _("ArgRange_Array"));
		}
		if (count < 0 || count > (chars.Length - index))
		{
			throw new ArgumentOutOfRangeException
				("count", _("ArgRange_Array"));
		}
		return InternalGetByteCount (chars, index, count);
	}

	// Convenience wrappers for "GetByteCount".
	public override int GetByteCount (String s)
	{
		if (s == null)
		{
			throw new ArgumentNullException ("s");
		}
		return InternalGetByteCount (s, 0, s.Length);
	}

	// Get the bytes that result from encoding a character buffer.
	public override int GetBytes (char[] chars, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
	{
		if (chars == null)
		{
			throw new ArgumentNullException ("chars");
		}
		if (bytes == null)
		{
			throw new ArgumentNullException ("bytes");
		}
		if (charIndex < 0 || charIndex > chars.Length)
		{
			throw new ArgumentOutOfRangeException
				("charIndex", _("ArgRange_Array"));
		}
		if (charCount < 0 || charCount > (chars.Length - charIndex))
		{
			throw new ArgumentOutOfRangeException
				("charCount", _("ArgRange_Array"));
		}
		if (byteIndex < 0 || byteIndex > bytes.Length)
		{
			throw new ArgumentOutOfRangeException
				("byteIndex", _("ArgRange_Array"));
		}
		int result = InternalGetBytes (chars, charIndex, charCount,
									  bytes, byteIndex);
		if (result != -1)
		{
			return result;
		}
		throw new ArgumentException (_("Arg_InsufficientSpace"));
	}

	// Convenience wrappers for "GetBytes".
	public override int GetBytes (String s, int charIndex, int charCount,
								 byte[] bytes, int byteIndex)
	{
		if (s == null)
		{
			throw new ArgumentNullException ("s");
		}
		if (bytes == null)
		{
			throw new ArgumentNullException ("bytes");
		}
		if (charIndex < 0 || charIndex > s.Length)
		{
			throw new ArgumentOutOfRangeException
				("charIndex", _("ArgRange_StringIndex"));
		}
		if (charCount < 0 || charCount > (s.Length - charIndex))
		{
			throw new ArgumentOutOfRangeException
				("charCount", _("ArgRange_StringRange"));
		}
		if (byteIndex < 0 || byteIndex > bytes.Length)
		{
			throw new ArgumentOutOfRangeException
				("byteIndex", _("ArgRange_Array"));
		}
		int result = InternalGetBytes (s, charIndex, charCount,
									  bytes, byteIndex);
		if (result != -1)
		{
			return result;
		}
		throw new ArgumentException (_("Arg_InsufficientSpace"));
	}
	public override byte[] GetBytes (String s)
	{
		if (s == null)
		{
			throw new ArgumentNullException ("s");
		}
		byte[] bytes = new byte [InternalGetByteCount (s, 0, s.Length)];
		InternalGetBytes (s, 0, s.Length, bytes, 0);
		return bytes;
	}

	// Get the number of characters needed to decode a byte buffer.
	public override int GetCharCount (byte[] bytes, int index, int count)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException ("bytes");
		}
		if (index < 0 || index > bytes.Length)
		{
			throw new ArgumentOutOfRangeException
				("index", _("ArgRange_Array"));
		}
		if (count < 0 || count > (bytes.Length - index))
		{
			throw new ArgumentOutOfRangeException
				("count", _("ArgRange_Array"));
		}
		return InternalGetCharCount (bytes, index, count);
	}

	// Get the characters that result from decoding a byte buffer.
	public override int GetChars (byte[] bytes, int byteIndex, int byteCount,
								 char[] chars, int charIndex)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException ("bytes");
		}
		if (chars == null)
		{
			throw new ArgumentNullException ("chars");
		}
		if (byteIndex < 0 || byteIndex > bytes.Length)
		{
			throw new ArgumentOutOfRangeException
				("byteIndex", _("ArgRange_Array"));
		}
		if (byteCount < 0 || byteCount > (bytes.Length - byteIndex))
		{
			throw new ArgumentOutOfRangeException
				("byteCount", _("ArgRange_Array"));
		}
		if (charIndex < 0 || charIndex > chars.Length)
		{
			throw new ArgumentOutOfRangeException
				("charIndex", _("ArgRange_Array"));
		}
		int result = InternalGetChars (bytes, byteIndex, byteCount,
									  chars, charIndex);
		if (result != -1)
		{
			return result;
		}
		throw new ArgumentException (_("Arg_InsufficientSpace"));
	}

	// Get the maximum number of bytes needed to encode a
	// specified number of characters.
	public override int GetMaxByteCount (int charCount)
	{
		if (charCount < 0)
		{
			throw new ArgumentOutOfRangeException
				("charCount", _("ArgRange_NonNegative"));
		}
		return InternalGetMaxByteCount (charCount);
	}

	// Get the maximum number of characters needed to decode a
	// specified number of bytes.
	public override int GetMaxCharCount (int byteCount)
	{
		if (byteCount < 0)
		{
			throw new ArgumentOutOfRangeException
				("byteCount", _("ArgRange_NonNegative"));
		}
		return InternalGetMaxCharCount (byteCount);
	}

	// Decode a buffer of bytes into a string.
	public override String GetString (byte[] bytes, int index, int count)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException ("bytes");
		}
		if (index < 0 || index > bytes.Length)
		{
			throw new ArgumentOutOfRangeException
				("index", _("ArgRange_Array"));
		}
		if (count < 0 || count > (bytes.Length - index))
		{
			throw new ArgumentOutOfRangeException
				("count", _("ArgRange_Array"));
		}
		return InternalGetString (bytes, index, count);
	}
	public override String GetString (byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException ("bytes");
		}
		return InternalGetString (bytes, 0, bytes.Length);
	}

	// Internal methods that are used by the runtime engine to
	// provide the default encoding.  These may assume that their
	// arguments have been fully validated.

	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static int InternalGetByteCount (char[] chars, int index, int count);

	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static int InternalGetByteCount (String s, int index, int count);

	// Returns -1 if insufficient space in "bytes".
	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static int InternalGetBytes
				(char[] chars, int charIndex, int charCount,
				 byte[] bytes, int byteIndex);

	// Returns -1 if insufficient space in "bytes".
	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static int InternalGetBytes
				(String s, int charIndex, int charCount,
				 byte[] bytes, int byteIndex);

	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static int InternalGetCharCount
				(byte[] bytes, int index, int count);

	// Returns -1 if insufficient space in "chars".
	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static int InternalGetChars
				(byte[] bytes, int byteIndex, int byteCount,
				 char[] chars, int charIndex);

	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static int InternalGetMaxByteCount (int charCount);

	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static int InternalGetMaxCharCount (int byteCount);

	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static String InternalGetString (byte[] bytes, int index, int count);

	// Get the default code page number.  Zero if unknown.
	[MethodImpl (MethodImplOptions.InternalCall)]
	extern internal static int InternalCodePage ();

}; // class DefaultEncoding

}; // namespace System.Text
