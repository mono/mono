/*
 * Encoder.cs - Implementation of the "System.Text.Encoder" class.
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
using System.Runtime.InteropServices;

[Serializable]
public abstract class Encoder
{

	// Constructor.
	protected Encoder() {}

#if NET_2_0
	EncoderFallback fallback = new EncoderReplacementFallback ();
	EncoderFallbackBuffer fallback_buffer;

	public EncoderFallback Fallback {
		get { return fallback; }
		set {
			if (value == null)
				throw new ArgumentNullException ();
			fallback = value;
			fallback_buffer = null;
		}
	}

	public EncoderFallbackBuffer FallbackBuffer {
		get {
			if (fallback_buffer == null)
				fallback_buffer = Fallback.CreateFallbackBuffer ();
			return fallback_buffer;
		}
	}
#endif

	// Get the number of bytes needed to encode a buffer.
	public abstract int GetByteCount(char[] chars, int index,
									 int count, bool flush);

	// Get the bytes that result from decoding a buffer.
	public abstract int GetBytes(char[] chars, int charIndex, int charCount,
								 byte[] bytes, int byteIndex, bool flush);

#if NET_2_0
	[CLSCompliant (false)]
	public unsafe virtual int GetByteCount (char* chars, int charCount, bool flush)
	{
		char [] carr = new char [charCount];
		Marshal.Copy ((IntPtr) chars, carr, 0, charCount);
		return GetByteCount (carr, 0, charCount, flush);
	}

	[CLSCompliant (false)]
	public unsafe virtual int GetBytes (char* chars, int charCount,
		byte* bytes, int byteCount, bool flush)
	{
		char [] carr = new char [charCount];
		Marshal.Copy ((IntPtr) chars, carr, 0, charCount);
		byte [] barr = new byte [byteCount];
		Marshal.Copy ((IntPtr) bytes, barr, 0, byteCount);
		return GetBytes (carr, 0, charCount, barr, 0, flush);
	}

	public virtual void Reset ()
	{
		if (fallback_buffer != null)
			fallback_buffer.Reset ();
	}
#endif

}; // class Encoder

}; // namespace System.Text
