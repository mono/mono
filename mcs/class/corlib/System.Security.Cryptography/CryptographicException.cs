//
// System.Security.Cryptography.CryptographicException.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Runtime.Serialization;
#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Security.Cryptography {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
	public class CryptographicException : SystemException, _Exception {
#else
	public class CryptographicException : SystemException {
#endif
		public CryptographicException ()
			: base (Locale.GetText ("Error occured during a cryptographic operation."))
		{
			// default to CORSEC_E_CRYPTO
			// defined as EMAKEHR(0x1430) in CorError.h
			HResult = unchecked ((int)0x80131430);
		}

		public CryptographicException (int hr)
		{
			HResult = hr;
		}

		public CryptographicException (string message)
			: base (message)
		{
			HResult = unchecked ((int)0x80131430);
		}

		public CryptographicException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = unchecked ((int)0x80131430);
		}

		public CryptographicException (string format, string insert)
			: base (String.Format (format, insert))
		{
			HResult = unchecked ((int)0x80131430);
		}

		protected CryptographicException (SerializationInfo info, StreamingContext context)
			: base (info, context) 
		{
		}
	}
}
