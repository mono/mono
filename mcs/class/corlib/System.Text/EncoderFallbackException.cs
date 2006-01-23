//
// EncoderFallbackException.cs
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
	[Serializable]
	public sealed class EncoderFallbackException : ArgumentException
	{
		const string defaultMessage =
			"Failed to decode the input byte sequence to Unicode characters.";

		public EncoderFallbackException ()
			: this (null)
		{
		}

		public EncoderFallbackException (string message)
			: base (message)
		{
		}

		public EncoderFallbackException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		internal EncoderFallbackException (char charUnknown, int index)
			: base (null)
		{
			char_unknown = charUnknown;
			this.index = index;
		}

		internal EncoderFallbackException (char charUnknownHigh,
			char charUnknownLow, int index)
			: base (null)
		{
			char_unknown_high = charUnknownHigh;
			char_unknown_low = charUnknownLow;
			this.index = index;
		}

		char char_unknown, char_unknown_high, char_unknown_low;
		int index = - 1;

		public char CharUnknown {
			get { return char_unknown; }
		}

		public char CharUnknownHigh {
			get { return char_unknown_high; }
		}

		public char CharUnknownLow {
			get { return char_unknown_low; }
		}

		[MonoTODO]
		public int Index {
			get { return index; }
		}

		[MonoTODO]
		public bool IsUnknownSurrogate ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
