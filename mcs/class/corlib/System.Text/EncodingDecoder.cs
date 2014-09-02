//
// System.Text.EncodingDecoder.cs
//
// Authors:
//   Marcos Henrich (marcos.henrich@xamarin.com)
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;

namespace System.Text {

abstract class EncodingDecoder : Decoder {
	protected readonly Encoding encoding;

	// Constructor.
	protected EncodingDecoder (Encoding encoding)
	{
		this.encoding = encoding;
		var fallback = encoding.DecoderFallback;
		if (fallback != null)
			Fallback = fallback;
	}

	public unsafe override void Convert (
		byte* bytes, int byteCount,
		char* chars, int charCount, bool flush,
		out int bytesUsed, out int charsUsed, out bool completed)
	{
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (charCount < 0)
			throw new ArgumentOutOfRangeException ("charCount");
		if (byteCount < 0)
			throw new ArgumentOutOfRangeException ("byteCount");

		bytesUsed = encoding.GetByteCount(chars, charCount);

		if (bytesUsed > byteCount) {
			charsUsed = encoding.GetChars (bytes, byteCount, chars, charCount);
			bytesUsed = encoding.GetByteCount (chars, charsUsed);
		} else
			charsUsed = encoding.GetChars (bytes, bytesUsed, chars, charCount);
		

		completed = bytesUsed == byteCount;
	}

	public override void Convert (
		byte [] bytes, int byteIndex, int byteCount,
		char [] chars, int charIndex, int charCount, bool flush,
		out int bytesUsed, out int charsUsed, out bool completed)
	{
		if (chars == null)
			throw new ArgumentNullException ("chars");
		if (bytes == null)
			throw new ArgumentNullException ("bytes");
		if (charIndex < 0)
			throw new ArgumentOutOfRangeException ("charIndex");
		if (charCount < 0 || chars.Length < charIndex + charCount)
			throw new ArgumentOutOfRangeException ("charCount");
		if (byteIndex < 0)
			throw new ArgumentOutOfRangeException ("byteIndex");
		if (byteCount < 0 || bytes.Length < byteIndex + byteCount)
			throw new ArgumentOutOfRangeException ("byteCount");

		bytesUsed = encoding.GetByteCount(chars, charIndex, charCount);
			
		if (bytesUsed > byteCount) {
			charsUsed = encoding.GetChars (bytes, byteIndex, byteCount, chars, charIndex);
			bytesUsed = encoding.GetByteCount (chars, charIndex, charsUsed);
		} else
			charsUsed = encoding.GetChars (bytes, byteIndex, bytesUsed, chars, charIndex);

		completed = bytesUsed == byteCount;
	}
}; // class EncodingDecoder

}; // namespace System.Text
