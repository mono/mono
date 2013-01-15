//
// HMACSHA384.cs: HMAC implementation using SHA384
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005, 2007 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	[ComVisible (true)]
	public class HMACSHA384 : HMAC {

		static bool legacy_mode;
		private bool legacy;

		static HMACSHA384 ()
		{
			legacy_mode = (Environment.GetEnvironmentVariable ("legacyHMACMode") == "1");
		}

		public HMACSHA384 () 
			: this (KeyBuilder.Key (8))
		{
			ProduceLegacyHmacValues = legacy_mode;
		}

		public HMACSHA384 (byte[] key)
		{
			ProduceLegacyHmacValues = legacy_mode;
			HashName = "SHA384";
			HashSizeValue = 384;
			Key = key;
		}

		// this property will appear in the next .NET service pack
		// http://blogs.msdn.com/shawnfa/archive/2007/01/31/please-do-not-use-the-net-2-0-hmacsha512-and-hmacsha384-classes.aspx
		public bool ProduceLegacyHmacValues {
			get { return legacy; }
			set {
				legacy = value;
				BlockSizeValue = legacy ? 64 : 128;
			}
		}
	}
}
