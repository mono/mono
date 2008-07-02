//
// System.Security.Cryptography.SHA1Managed.cs
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
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

using System.Runtime.InteropServices;

namespace System.Security.Cryptography {

// Note:
// The MS Framework includes two (almost) identical class for SHA1.
//	SHA1Managed (this file) is a 100% managed implementation.
//	SHA1CryptoServiceProvider is a wrapper on CryptoAPI.
// Mono must provide those two class for binary compatibility.
// In our case both class are wrappers around a managed internal class SHA1Internal.

#if NET_2_0
	[ComVisible (true)]
#endif
	public class SHA1Managed : SHA1 {

		private SHA1Internal sha;

		public SHA1Managed () 
		{
			sha = new SHA1Internal ();
		}

		protected override void HashCore (byte[] rgb, int ibStart, int cbSize) 
		{
			State = 1;
			sha.HashCore (rgb, ibStart, cbSize);
		}

		protected override byte[] HashFinal () 
		{
			State = 0;
			return sha.HashFinal ();
		}

		public override void Initialize () 
		{
			sha.Initialize ();
		}
	}
}
