//
// DecoderReplacementFallbackBuffer.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 || NET_2_0_BOOTSTRAP

namespace System.Text
{
	// This DecoderFallbackBuffer is simple. It ignores the input buffers.
	// DecoderFallbackBuffer users could implement their own complex 
	// fallback buffers.

	public sealed class DecoderReplacementFallbackBuffer
		: DecoderFallbackBuffer
	{
		bool fallback_assigned;
		int current;
		string replacement;

		public DecoderReplacementFallbackBuffer (
			DecoderReplacementFallback fallback)
		{
			if (fallback == null)
				throw new ArgumentNullException ("fallback");
			replacement = fallback.DefaultString;
			current = 0;
		}

		public override int Remaining {
			get { return fallback_assigned ? replacement.Length - current : 0; }
		}

		public override bool Fallback (byte [] bytesUnknown, int index)
		{
			if (bytesUnknown == null)
				throw new ArgumentNullException ("bytesUnknown");
			if (fallback_assigned && Remaining != 0)
				throw new ArgumentException ("Reentrant Fallback method invocation occured. It might be because either this FallbackBuffer is incorrectly shared by multiple threads, invoked inside Encoding recursively, or Reset invocation is forgotten.");
			if (index < 0 || bytesUnknown.Length < index)
				throw new ArgumentOutOfRangeException ("index");
			fallback_assigned = true;
			current = 0;

			return replacement.Length > 0;
		}

		public override char GetNextChar ()
		{
			if (!fallback_assigned)
				return '\0';
			if (current >= replacement.Length)
				return char.MinValue;
			return replacement [current++];
		}

		public override bool MovePrevious ()
		{
			if (current == 0)
				return false;
			current--;
			return true;
		}

		public override void Reset ()
		{
			fallback_assigned = false;
			current = 0;
		}
	}
}

#endif
